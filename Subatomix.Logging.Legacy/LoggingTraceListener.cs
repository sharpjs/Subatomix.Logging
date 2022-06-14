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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Subatomix.Logging.Legacy;

using static MethodImplOptions;
using F = Formatters;

/// <summary>
///   A trace listener that forwards trace events to <see cref="ILogger"/>
///   loggers.
/// </summary>
public class LoggingTraceListener : TraceListener
{
    internal const string DefaultLoggerCategory = "Trace";

    private ILoggerFactory _loggerFactory;

    private readonly StringBuilder        _buffer;
    private readonly ReaderWriterLockSlim _bufferLock;

    /// <summary>
    ///   Initializes a new <see cref="LoggingTraceListener"/> instance.
    /// </summary>
    public LoggingTraceListener()
    {
        _loggerFactory = NullLoggerFactory.Instance;
        _buffer        = new StringBuilder();
        _bufferLock    = new ReaderWriterLockSlim();
    }

    /// <summary>
    ///   Gets or sets the provider of loggers.
    /// </summary>
    public ILoggerFactory LoggerFactory
    {
        get => _loggerFactory;
        set => _loggerFactory = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc/>
    public override bool IsThreadSafe => true;

    /// <inheritdoc/>
    public override void TraceEvent(
        TraceEventCache? context,
        string           source,
        TraceEventType   type,
        int              id)
    {
        TraceEvent(context, source, type, id, "Message ID: {0}", id);
    }

    /// <inheritdoc/>
    public override void TraceEvent(
        TraceEventCache? context,
        string           source,
        TraceEventType   type,
        int              id,
        string?          message)
    {
        if (!ShouldTrace(context, source, type, id, message))
            return;

        Flush();

        var logger = GetLogger(source);
        var level  = type.ToLogLevel();

        logger.Log(level, id, message, null, F.Message);
    }

    /// <inheritdoc/>
    public override void TraceEvent(
        TraceEventCache?  context,
        string            source,
        TraceEventType    type,
        int               id,
        string?           template,
        params object?[]? args)
    {
        if (!ShouldTrace(context, source, type, id, template, args))
            return;

        Flush();

        var logger = GetLogger(source);
        var level  = type.ToLogLevel();

        logger.Log(level, id, (template, args), null, F.Template);
    }

    /// <inheritdoc/>
    public override void TraceTransfer(
        TraceEventCache? context,
        string           source,
        int              id,
        string?          message,
        Guid             relatedActivityId)
    {
        TraceEvent(
            context, source, TraceEventType.Transfer, id,
            "{0} {{related:{1}}}", message, relatedActivityId
        );
    }

    /// <inheritdoc/>
    public override void TraceData(
        TraceEventCache? context,
        string           source,
        TraceEventType   type,
        int              id,
        object?          obj)
    {
        if (!ShouldTrace(context, source, type, id, obj: obj))
            return;

        Flush();

        var logger = GetLogger(source);
        var level  = type.ToLogLevel();

        if (obj is Exception exception)
            logger.Log(level, id, default, exception, F.Empty);
        else
            logger.Log(level, id, obj, null, F.Data);
    }

    /// <inheritdoc/>
    public override void TraceData(
        TraceEventCache?  context,
        string            source,
        TraceEventType    type,
        int               id,
        params object?[]? objs)
    {
        if (!ShouldTrace(context, source, type, id, objs: objs))
            return;

        Flush();

        var logger = GetLogger(source);
        var level  = type.ToLogLevel();

        if (objs is { Length: 1 } && objs[0] is Exception exception)
            logger.Log(level, id, default, exception, F.Empty);
        else
            logger.Log(level, id, objs, null, F.DataArray);
    }

    /// <inheritdoc/>
    public override void Fail(string? message)
    {
        Flush();

        GetLogger().Log(LogLevel.Error, 0, message, null, F.Message);
    }

    /// <inheritdoc/>
    public override void Fail(string? message, string? detailMessage)
    {
        Flush();

        GetLogger().Log(LogLevel.Error, 0, (message, detailMessage), null, F.MessageAndDetail);
    }

    /// <inheritdoc/>
    public override void Write(string? message)
    {
        _bufferLock.EnterWriteLock();
        try
        {
            _buffer.Append(message);
        }
        finally
        {
            _bufferLock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override void WriteLine(string? message)
    {
        _bufferLock.EnterWriteLock();
        try
        {
            _buffer.AppendLine(message);
        }
        finally
        {
            _bufferLock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        // GetBufferedText returns null or a non-empty string
        if (GetBufferedText() is not { } text)
            return;

        GetLogger().Log(LogLevel.Debug, 0, text, null, F.Message);
    }

    /// <inheritdoc/>
    public override void Close()
    {
        Flush();

        LoggerFactory = NullLoggerFactory.Instance;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool managed)
    {
        if (managed)
        {
            Close();

            _buffer.Clear();
            _buffer.Capacity = 0;
            _bufferLock.Dispose();
        }

        base.Dispose(managed);
    }

    private ILogger GetLogger(string? source = null)
    {
        return _loggerFactory.CreateLogger(
            GetFirstNotNullOrEmpty(source, Name, DefaultLoggerCategory)
        );
    }

    private static string GetFirstNotNullOrEmpty(string? a, string? b, string c)
    {
        if (!string.IsNullOrEmpty(a))
            return a!;

        if (!string.IsNullOrEmpty(b))
            return b!;

        return c;
    }

    private string? GetBufferedText()
    {
        // This is a typical double-check pattern using ReaderWriterLockSlim

        // Check in read mode for an empty buffer, the usual case
        _bufferLock.EnterReadLock();
        try
        {
            // Multiple threads can be in this block at the same time.
            // Do reads only.

            if (_buffer.Length == 0)
                return null;
        }
        finally
        {
            _bufferLock.ExitReadLock();
        }

        // Buffer is not empty
        return TakeBufferedText();
    }

    // Uncoverable: Cannot reliably recreate the timing required to exercise
    // the `return null` case
    [ExcludeFromCodeCoverage]
    [MethodImpl(AggressiveInlining)]
    private string? TakeBufferedText()
    {
        // Check again in upgradable read mode; buffer might have changed
        _bufferLock.EnterUpgradeableReadLock();
        try
        {
            // Only one thread can be in this block, but other threads stil
            // can be in GetBufferedText at the same time.  Do reads only.

            if (_buffer.Length == 0)
                return null;

            // Buffer still is not empty
            return TakeBufferedTextCore();
        }
        finally
        {
            _bufferLock.ExitUpgradeableReadLock();
        }
    }

    [MethodImpl(AggressiveInlining)]
    private string TakeBufferedTextCore()
    {
        // Take all the text from the buffer, clearing the buffer
        _bufferLock.EnterWriteLock();
        try
        {
            // Exclusive lock.  Only one thread can be in this block.  No other
            // thread will be in any block protected by this lock.  Reads and
            // writes are OK.

            var text = _buffer.ToString();
            _buffer.Clear();

            return text;
        }
        finally
        {
            _bufferLock.ExitWriteLock();
        }
    }

    [MethodImpl(AggressiveInlining)]
    private bool ShouldTrace(
        TraceEventCache? e,
        string           source,
        TraceEventType   type,
        int              id,
        string?          message = null,
        object?[]?       args    = null,
        object?          obj     = null,
        object?[]?       objs    = null)
    {
        return Filter is null
            || Filter.ShouldTrace(e, source, type, id, message, args, obj, objs);
    }
}
