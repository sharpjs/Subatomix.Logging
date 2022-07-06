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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Subatomix.Logging.Sql;

/// <summary>
///   A provider of <see cref="SqlLogger"/> instances.
/// </summary>
[ProviderAlias("Sql")]
public class SqlLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _queue;
    private readonly SqlCommand                _command;
    private readonly AutoResetEvent            _flushEvent;
    private readonly Thread                    _flushThread;
    private readonly IDisposable               _optionsChangeToken;

    // Used by flush thread
    private SqlConnection? _connection;
    private DateTime       _flushTime;      // time of next flush cycle
    private DateTime       _retryTime;      // time when resume flushing (or zero)
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

        _queue       = new();
        _command     = CreateFlushCommand();
        _flushEvent  = new(initialState: false);
        _flushThread = CreateFlushThread();

        Options = options.CurrentValue;

        _optionsChangeToken = options.OnChange(Configure);
        _flushThread.Start();
    }

    /// <summary>
    ///   Gets the most recently set options.
    /// </summary>
    public SqlLoggerOptions Options { get; private set; }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => new SqlLogger(this, categoryName);

    /// <summary>
    ///   Gets or sets the logger to use for diagnostic messages from the
    ///   logger provider itself.
    /// </summary>
    public ILogger Logger { get; set; }
        = NullLogger.Instance;

    internal void Enqueue(LogEntry entry)
        => _queue.Enqueue(entry);

    /// <summary>
    ///   Flushes buffered entries to the database.
    /// </summary>
    public void Flush()
        => _flushEvent.Set();

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    // For testing
    internal void SimulateUnmanagedDisposal()
        => Dispose(managed: false);

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
        _flushEvent        .Dispose();
        _command           .Dispose();
        _optionsChangeToken.Dispose();
    }

    private Thread CreateFlushThread()
    {
        return new(FlushThreadMain)
        {
            Name         = nameof(SqlLoggerProvider) + ".Flush",
            Priority     = ThreadPriority.AboveNormal,
            IsBackground = true, // don't prevent app from exiting
        };
    }

    private SqlCommand CreateFlushCommand()
    {
        var command = new SqlCommand()
        {
            CommandType = CommandType.StoredProcedure,
            CommandText = "log.Write",
            // CommandTimeout set later
        };

        _command.Parameters
            .Add("@LogName", SqlDbType.VarChar, 128);
            // Value set later

        _command.Parameters
            .Add("@MachineName", SqlDbType.VarChar, 255)
            .Value = Environment.MachineName.Truncate(255);

        _command.Parameters
            .Add("@ProcessId", SqlDbType.Int)
            .Value = Process.GetCurrentProcess().Id;

        _command.Parameters
            .Add("@EntryRows", SqlDbType.Structured)
            .TypeName = "log.EntryRow";

        return command;
    }

    private void Configure(SqlLoggerOptions options)
    {
        Options = options;
    }

    private static string Truncate(string s, int length)
        => s.Length <= length ? s : s.Substring(0, length);

    #region Flush Thread

    private void FlushThreadMain()
    {
        ScheduleNextFlush();

        for (var retries = 0;;)
        {
            try
            {
                if (_exiting)
                    return;

                if (WaitUntilFlush())
                    // Normal flush
                    FlushAll();
                else
                    // In retry backoff, prune queue instead
                    Prune();

                ClearRetry(ref retries);
            }
            catch (Exception e)
            {
                OnException(e);
                ScheduleRetry(ref retries);
            }
        }
    }

    private void ScheduleNextFlush()
    {
        var now = DateTime.UtcNow;

        // Schedule the usual flush
        _flushTime = now + Options.AutoflushWait;

        // If not in retry backoff, flush as scheduled
        if (_retryTime < now)
            return;

        // If retry scheduled before flush, reschedule flush to coincide
        if (_flushTime > _retryTime)
            _flushTime = _retryTime;

        // NOTE: If flush occurs during retry backoff, the flush becomes a prune
    }

    private bool WaitUntilFlush()
    {
        // Compute how long to wait
        var duration = _flushTime - DateTime.Now;

        // Wait
        var interrupted
            =  duration > TimeSpan.Zero
            && _flushEvent.WaitOne(duration);

        // Decide what to do: flush or prune?
        var shouldFlush
            =  interrupted                  // flush if explicit Flush() invoked
            || _flushTime >= _retryTime;    // flush if not in retry backoff

        ScheduleNextFlush();
        return shouldFlush;
    }

    private void ClearRetry(ref int retries)
    {
        _retryTime = default;
        retries    = 0;
    }

    private void ScheduleRetry(ref int retries)
    {
        // Adaptive backoff: nothing for the first retry, then increasing by
        // regular increments for each successive retry, up to some maximum.

        var backoff = new TimeSpan(Math.Min(
            Options.RetryWaitIncrement.Ticks * retries,
            Options.RetryWaitMax.Ticks
        ));

        Logger.LogWarning("Flush failed; retrying after {delay}.", backoff);

        // Schedule retry
        _retryTime = DateTime.UtcNow + backoff;

        // If retry scheduled before next flush, reschedule flush to coincide
        if (_flushTime > _retryTime)
            _flushTime = _retryTime;

        if (retries < int.MaxValue)
            retries++;
    }

    private void FlushAll()
    {
        // Avoid flushing an empty queue
        if (_queue.IsEmpty)
            return;

        if (TryEnsureConnection())
            DoBatched(GetSnapshot(), Options.BatchSize, FlushBatch);
        else
            Prune();
    }

    private LogEntry[] GetSnapshot()
    {
        var entries = _queue.ToArray();

        var ordinal = 0;
        foreach (var e in entries)
            e.Ordinal = ordinal++;

        return entries;
    }

    private bool TryEnsureConnection()
    {
        var connection       = _connection;
        var connectionString = Options.ConnectionString;

        // If connection is open and current, use it
        if (connection is { State: ConnectionState.Open })
            if (connection.ConnectionString == connectionString)
                return true;

        // Connection is broken, stale, or null; dispose it
        connection?.Dispose();
        _connection = null;

        // Require connection string
        if (connectionString is not { Length: > 0 })
            return false;

        // Set up new connection
        connection          = new(connectionString);
        _connection         = connection;
        _command.Connection = connection;

        connection.Open();
        return true;
    }

    private void FlushBatch(ArraySegment<LogEntry> entries)
    {
        Write  (entries);
        Dequeue(entries.Count);
    }

    private void Write(ArraySegment<LogEntry> entries)
    {
        _command.Parameters[0].Value = Options.LogName;
        _command.Parameters[3].Value = new ObjectDataReader<LogEntry>(entries, LogEntry.Map);
        _command.CommandTimeout      = (int) Options.BatchTimeout.TotalSeconds;
        _command.ExecuteNonQuery();
    }

    private void Dequeue(int count)
    {
        for (; count > 0; count--)
            _queue.TryDequeue(out _);
    }

    private void OnException(Exception e)
    {
        Logger.LogError(e);
    }

    private void Prune()
    {
        var count = _queue.Count - Options.MaxQueueSize;

        for (; count > 0; count--)
            _queue.TryDequeue(out _);
    }

    private static void DoBatched<T>(
        T[]                     items,
        int                     batchSize,
        Action<ArraySegment<T>> action)
    {
        var start     = 0;
        var remaining = items.Length;

        while (remaining > batchSize)
        {
            var batch = new ArraySegment<T>(items, start, batchSize);
            action(batch);

            // Allow done items to be garbage-collected
            Array.Clear(items, start, batchSize);

            start     += batchSize;
            remaining -= batchSize;
        }

        if (remaining > 0)
        {
            var batch = new ArraySegment<T>(items, start, remaining);
            action(batch);
        }
    }

    #endregion
}
