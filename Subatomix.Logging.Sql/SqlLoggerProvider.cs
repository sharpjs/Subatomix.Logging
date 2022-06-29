/*
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System.Collections.Concurrent;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Subatomix.Logging.Sql;

/// <summary>
///   A provider of <see cref="SqlLogger"/> instances.
/// </summary>
[ProviderAlias("Sql")]
public class SqlLoggerProvider : ILoggerProvider
{
    private const int
        MaxMessageLength      = 1024,
        CommandTimeoutSeconds = 3 * 60; // 3 minutes

    private const string
        WriteCommandName                     = "WriteLog",
        WriteCommandRowsParameterName        = "@EntryRows",
        WriteCommandRowsParameterSqlDataType = "dbo.LogEntryRow";

    private readonly ConcurrentQueue<LogEntry> _queue;
    private readonly SqlCommand                _command;
    private readonly AutoResetEvent            _flushEvent;
    private readonly Thread                    _flushThread;
    private readonly IDisposable               _optionsChangeToken;

    // Used by flush thread
    private SqlConnection? _connection;
    private DateTime       _flushTime;
    private bool           _exiting;

    /// <summary>
    ///   Initializes a new <see cref="SqlLoggerProvider"/> instance with the
    ///   the specified options.
    /// </summary>
    /// <param name="options">
    ///   The options for the provider.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public SqlLoggerProvider(IOptionsMonitor<SqlLoggerOptions> options)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        Options = options.CurrentValue;
        Configure();

        _optionsChangeToken = options.OnChange(Configure);

        _queue   = new();
        _command = CreateFlushCommand();

        _flushEvent  = new(initialState: false);
        _flushThread = CreateFlushThread();
        _flushThread.Start();
    }

    /// <summary>
    ///   Gets the most recently set options.
    /// </summary>
    public SqlLoggerOptions Options { get; private set; }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new SqlLogger(this, categoryName);
    }

    internal void Enqueue(LogEntry entry)
    {
        if (entry.Message is { } m)
            entry.Message = Truncate(m, MaxMessageLength);
        _queue.Enqueue(entry);
    }

    /// <summary>
    ///   Flushes buffered events to the database.
    /// </summary>
    public void Flush()
    {
        _flushEvent.Set();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Disposes resources owned by the object.
    /// </summary>
    /// <param name="managed">
    ///   Whether to dispose managed resources
    ///   (in addition to unmanaged resources, which are always disposed).
    /// </param>
    protected virtual void Dispose(bool managed)
    {
        if (!managed || _exiting)
            return;

        // Tell flusher thread to flush and exit
        _exiting = true;
        _flushEvent.Set();

        // Give flusher thread a fair chance to flush
// #if NETFRAMEWORK
//         if (!_flushThread.Join(Options.CloseWait))
//             _flushThread.Abort();
// #else
        _flushThread.Join();
//#endif

        // Dispose managed objects
        _flushEvent.Dispose();
        _command.Dispose();
        _optionsChangeToken.Dispose();
    }

    private void Configure(SqlLoggerOptions options)
    {
        Options = options;
        Configure();
    }

    private void Configure()
    {
    }

    private Thread CreateFlushThread()
    {
        return new(FlushThreadMain)
        {
            Name         = "SqlTraceListener.Flush",
            Priority     = ThreadPriority.AboveNormal,
            IsBackground = true, // don't prevent app from exiting
        };
    }

    private SqlCommand CreateFlushCommand()
    {
        var command = new SqlCommand()
        {
            CommandType    = CommandType.StoredProcedure,
            CommandText    = "WriteLog",
            CommandTimeout = CommandTimeoutSeconds
        };

        var process = Process.GetCurrentProcess();

        _command.Parameters
            .Add("@LogName", SqlDbType.VarChar, 128)
            .Value = process.ProcessName.Truncate(128);

        _command.Parameters
            .Add("@MachineName", SqlDbType.VarChar, 255)
            .Value = Environment.MachineName.Truncate(255);

        _command.Parameters
            .Add("@ProcessId", SqlDbType.Int)
            .Value = process.Id;

        _command.Parameters
            .Add("@EntryRows", SqlDbType.Structured)
            .TypeName = "dbo.LogEntryRow";

        return command;
    }

    private static string Truncate(string s, int length)
        => s.Length <= length ? s : s.Substring(0, length);

    #region Flush Thread

    private void FlushThreadMain()
    {
        ScheduleAutoflush();

        for (var retries = 0;;)
        {
            try
            {
                if (_exiting)
                    return;

                WaitBeforeFlush();
                FlushCore();

                retries = 0;
            }
            catch (Exception e)
            {
                OnException(e);
                WaitBeforeRetry(retries);

                if (retries < int.MaxValue)
                    retries++;
            }
        }
    }

    private void ScheduleAutoflush()
    {
        _flushTime = DateTime.UtcNow + Options.AutoflushWait;
    }

    private void WaitBeforeFlush()
    {
        // Compute time remaining until autoflush
        var duration = _flushTime - DateTime.UtcNow;

        // Wait until autoflush time, or until Flush() called
        if (duration > TimeSpan.Zero)
            _flushEvent.WaitOne(duration);

        // Compute time of next autoflush
        ScheduleAutoflush();
    }

    private void FlushCore()
    {
        // Avoid flushing an empty queue
        var queue = _queue;
        if (queue.IsEmpty)
            return;

        // Take a snapshot of the pending events at this time
        var events = queue.ToArray();

        // Prepare queue snapshot for transmission
        RenumberEvents(events);

        // Ensure good connection to the server
        var connection = EnsureConnection();

        // Transmit the events to the server
        FlushCore(events);

        // Remove the events from the queue
        for (var count = events.Length; count > 0; count--)
            queue.TryDequeue(out var entry);
    }

    private SqlConnection EnsureConnection()
    {
        var connection       = _connection;
        var connectionString = Options.ConnectionString;

        // If connection is open and current, use it
        if (connection is { State: ConnectionState.Open })
            if (connection.ConnectionString == connectionString)
                return connection;

        // If connection is broken or stale, dispose it
        if (connection is { })
            connection.Dispose();

        // Set up a new connection
        connection          = new(connectionString); //.Logged();
        _connection         = connection;
        _command.Connection = connection;

        connection.Open();

        return connection;
    }

    private void FlushCore(LogEntry[] events)
    {
        _command.Parameters[3].Value = new ObjectDataReader<LogEntry>(events, LogEntry.Map);
        _command.ExecuteNonQuery();
    }

    private void OnException(Exception e)
    {
        //TraceSource.TraceError(e);
    }

    private void WaitBeforeRetry(int retries)
    {
        // Adaptive delay: nothing for the first retry, then increasing by
        // regular increments for each successive retry, up to some maximum.
        // Default: wait 5 minutes longer for each retry, up to 1 hour max.

        var incrementsMax = Options.RetryWaitMax.Ticks / Options.RetryWaitIncrement.Ticks;
        var increments    = Math.Min((long) retries, incrementsMax);
        var duration      = new TimeSpan(increments * Options.RetryWaitIncrement.Ticks);

        //TraceSource.TraceWarning("Flush failed; retrying after {0:c}.", duration);

        Thread.Sleep(duration);
    }

    private static void RenumberEvents(LogEntry[] events)
    {
        var ordinal = 0;
        foreach (var e in events)
            e.Ordinal = ordinal++;
    }

    #endregion
}
