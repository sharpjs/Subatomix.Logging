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

namespace Subatomix.Logging.Sql;

/// <summary>
///   Options for <see cref="SqlLoggerProvider"/> and <see cref="SqlLogger"/>.
/// </summary>
public class SqlLoggerOptions
{
    /// <summary>
    ///   Gets or sets the database connection string.  The default is
    ///   <see langword="null"/>.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    ///   Gets or sets the name of the log stream.  The default is the name of
    ///   the current operating system process.
    /// </summary>
    /// <remarks>
    ///   The log stream name is an application-defined arbitrary value used to
    ///   differentiate the application's log entries from those of other
    ///   applications written to the same database.
    /// </remarks>
    public string LogName { get; set; }
        = Process.GetCurrentProcess().ProcessName;

    /// <summary>
    ///   Gets or sets the maximum count of log entries that can be pending to
    ///   be flushed to the database.  The default is 10,000,000 entries.
    /// </summary>
    /// <remarks>
    ///   If a new log entry would cause the count of pending entries to exceed
    ///   the maximum queue size, the provider drops the oldest log entry so
    ///   that the count never exceeds this limit.
    /// </remarks>
    public int MaxQueueSize { get; set; }
        = 10_000_000;

    /// <summary>
    ///   Gets or sets the maximum count of log entries to flush to the
    ///   database in a single batch.  The default is 10,000 entries.
    /// </summary>
    /// <remarks>
    ///   If the count of pending log entries is greater than the batch size,
    ///   the provider will flush in multiple batches.
    /// </remarks>
    public int BatchSize { get; set; }
        = 10_000;

    /// <summary>
    ///   Gets or sets the maximum duration in which to flush a single batch of
    ///   entries to the database.  The default is 30 seconds.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If a flush operation times out, the provider abandons the failed
    ///     operation and schedules a retry.
    ///   </para>
    ///   <para>
    ///     NOTE: In the event of a timeout, SqlClient sends a signal to the
    ///     database server to cancel the executing command.  In the rare case
    ///     that the server does not receive the signal, it is possible that
    ///     the command will continue to execute on the server.
    ///   </para>
    /// </remarks>
    public TimeSpan BatchTimeout { get; set; }
        = TimeSpan.FromSeconds(30);

    /// <summary>
    ///   Gets or sets the duration to wait before the next flush of log
    ///   entries to the database after a successful flush.  The default is 5
    ///   seconds.
    /// </summary>
    public TimeSpan AutoflushWait { get; set; }
        = TimeSpan.FromSeconds(5);

    /// <summary>
    ///   Gets or sets the increment to add, after each consecutive failure, to
    ///   the duration to wait before retrying to flush log entries to the
    ///   database.  The default is 30 seconds.
    /// </summary>
    public TimeSpan RetryWaitIncrement { get; set; }
        = TimeSpan.FromSeconds(30);

    /// <summary>
    ///   Gets or sets the maximum duration to wait after a failure before
    ///   retrying to flush log entries to the database.  The default is 1
    ///   hour.
    /// </summary>
    public TimeSpan RetryWaitMax { get; set; }
        = TimeSpan.FromHours(1);

    /// <summary>
    ///   Gets or sets the maximum duration to wait for log entries to flush to
    ///   the database during disposal.  The default is 1 minute.
    /// </summary>
    public TimeSpan ShutdownWait { get; set; }
        = TimeSpan.FromMinutes(1);
}
