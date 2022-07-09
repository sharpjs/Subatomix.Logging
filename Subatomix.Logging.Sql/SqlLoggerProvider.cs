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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Subatomix.Logging.Internal;

namespace Subatomix.Logging.Sql;

/// <summary>
///   A provider of <see cref="SqlLogger"/> instances.
/// </summary>
[ProviderAlias("Sql")]
public class SqlLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentQueue<LogEntry> _queue;
    private readonly AutoResetEvent            _flushEvent;
    private readonly Thread                    _flushThread;
    private readonly ISqlLogRepository         _repository;
    private readonly IClock                    _clock;
    private readonly IDisposable               _optionsChangeToken;

    // Used by flush thread
    private DateTime _flushTime;    // time of next flush thread run
    private DateTime _retryTime;    // time when retry backoff ends
    private int      _exiting;      // because Interlocked doesn't do bool

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
        : this(options, new SqlLogRepository(), UtcClock.Instance)
    { }

    // For testing
    internal SqlLoggerProvider(
        IOptionsMonitor<SqlLoggerOptions> options,
        ISqlLogRepository                 repository,
        IClock                            clock,
        bool                              startThread = true)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        _queue       = new();
        _repository  = repository;
        _clock       = clock;
        _flushEvent  = new(initialState: false);
        _flushThread = CreateFlushThread();

        Options = options.CurrentValue;

        _optionsChangeToken = options.OnChange(Configure);

        if (startThread)
            _flushThread.Start();
    }

    /// <summary>
    ///   Gets the most recently set options.
    /// </summary>
    public SqlLoggerOptions Options { get; private set; }

    /// <summary>
    ///   Gets or sets the logger to use for diagnostic messages from the
    ///   logger provider itself.
    /// </summary>
    public ILogger Logger { get; set; }
        = NullLogger.Instance;

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => new SqlLogger(this, categoryName);

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
        if (!managed)
            return;

        if (Interlocked.Exchange(ref _exiting, -1) != 0)
            return;

        // Tell flusher thread to flush and exit
        _flushEvent.Set();

        // Give flusher thread a fair chance to flush
// #if NETFRAMEWORK
//         if (!_flushThread.Join(Options.CloseWait))
//             _flushThread.Abort();
// #else
        if (_flushThread.IsAlive)
            _flushThread.Join();
//#endif

        // Dispose managed objects
        _flushEvent        .Dispose();
        _repository        .Dispose();
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

    private void Configure(SqlLoggerOptions options)
    {
        Options = options;
    }

    #region Flush Thread

    private void FlushThreadMain()
    {
        ScheduleNextFlush();

        var retries = 0;

        while (Tick(ref retries)) { }
    }

    internal bool Tick(ref int retries)
    {
        try
        {
            if (Volatile.Read(ref _exiting) != 0)
                return false;

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

        return true;
    }

    private void ScheduleNextFlush()
    {
        var now = _clock.Now;

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
        var duration = _flushTime - _clock.Now;

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
        _retryTime = _clock.Now + backoff;

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

        if (_repository.TryEnsureConnection(Options.ConnectionString))
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

    private void FlushBatch(ArraySegment<LogEntry> entries)
    {
        Write  (entries);
        Dequeue(entries.Count);
    }

    private void Write(ArraySegment<LogEntry> entries)
    {
        _repository.Write(Options.LogName, entries, Options.BatchTimeout);
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
