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
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics;

#if NETFRAMEWORK
// Prevent CS1574: XML comment has cref attribute '...' that could not be resolved
using LogLevel = LogLevel;
#endif

/// <summary>
///   A scope representing a logical operation, intended for use with the
///   <see cref="ILogger"/> API.
/// </summary>
/// <remarks>
///   <para>
///     On construction, a scope logs a start message containing the name of
///     the operation.  On disposal, a scope logs a completion message
///     containing the name of the operation and the duration that elapsed
///     between construction and disposal.
///   </para>
///   <para>
///     Code using this type can arrange for exception logging by setting the
///     <see cref="Exception"/> propery, typically in a <see langword="catch"/>
///     block.  On disposal, if a scope's <see cref="Exception"/> propery is
///     not <see langword="null"/>, the scope logs the exception immediately
///     before the completion message and adds an exception indicator to the
///     completion message.
///   </para>
/// </remarks>
public class OperationScope : IDisposable
{
    /// <summary>
    ///   Gets the logger for operation-related messages.
    /// </summary>
    protected internal ILogger Logger { get; }

    /// <summary>
    ///   Gets the severity level for operation start and completion messages.
    /// </summary>
    /// <remarks>
    ///   Use the <see cref="ExceptionLogLevel"/> to set the severity level
    ///   used to report the exception, if any, associated with the operation.
    /// </remarks>
    protected internal LogLevel LogLevel { get; }

    /// <summary>
    ///   Gets the name of the operation.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///   Gets or sets the exception associated with the operation, or
    ///   <see langword="null"/> if no exception is associated with the
    ///   operation.  The default is <see langword="null"/>.
    /// </summary>
    /// <remarks>
    ///   On disposal, if this property is not <see langword="null"/>, the
    ///   scope logs the exception immediately before the completion message
    ///   and adds an exception indicator to the completion message.
    /// </remarks>
    public Exception? Exception { get; set; }

    /// <summary>
    ///   Gets or sets the severity level used to report the exception, if any,
    ///   associated with the operation. The default is
    ///   <see cref="LogLevel.Error"/>.
    /// </summary>
    public LogLevel ExceptionLogLevel { get; set; } = LogLevel.Error;

    // Opaque scope returned by ILogger.BeginScope().
    private readonly IDisposable _logScope;

    // Measures the elapsed time of the operation.
    private readonly Stopwatch _stopwatch;

    /// <summary>
    ///   Initializes and starts a new <see cref="OperationScope"/> instance.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="logLevel">
    ///   The severity level for operation start and completion messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    public OperationScope(
        ILogger                   logger,
        LogLevel                  logLevel = LogLevel.Information,
        [CallerMemberName] string name     = null!)
    : this(logger, logLevel, name, start: true)
    { }

    /// <summary>
    ///   Initializes a new <see cref="OperationScope"/> instance.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="logLevel">
    ///   The severity level for operation start and completion messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="start">
    ///   <para>
    ///     Whether to invoke <see cref="Start"/>.
    ///   </para>
    ///   <para>
    ///     A constructor of a derived type can pass <see langword="false"/> to
    ///     permit further initialization before the logical operation starts.
    ///     The derived type constructor must then invoke <see cref="Start"/>
    ///     before returning.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="logger"/> and/or <paramref name="name"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    protected OperationScope(ILogger logger, LogLevel logLevel, string name, bool start)
    {
        if (logger is null)
            throw new ArgumentNullException(nameof(logger));
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (name.Length == 0)
            throw new ArgumentException("Operation name cannot be empty.", nameof(name));

        Logger   = logger;
        LogLevel = logLevel;
        Name     = name;

        _logScope  = logger.BeginScope(name);
        _stopwatch = new();

        if (start) Start();
    }

    /// <summary>
    ///   Marks the completion of the logical operation the scope represents.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     On disposal, a scope logs a completion message containing the name
    ///     of the operation and the duration that elapsed between construction
    ///     and disposal.
    ///   </para> 
    ///   <para>
    ///     On disposal, if a scope's <see cref="Exception"/> propery is not
    ///     <see langword="null"/>, the scope logs the exception immediately
    ///     before the completion message and adds an exception indicator to
    ///     the completion message.
    ///   </para>
    ///   <para>
    ///     Note that <see cref="OperationScope"/>'s <see cref="IDisposable"/>
    ///     implementation assumes that no scope will need to dispose an
    ///     unmanaged resource.
    ///   </para> 
    /// </remarks>
    void IDisposable.Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Starts the logical operation.
    /// </summary>
    protected virtual void Start()
    {
        LogStarting();
        _stopwatch.Start();
    }

    /// <summary>
    ///   Stops the logical operation and disposes managed resources owned by
    ///   the scope.
    /// </summary>
    protected virtual void Stop()
    {
        if (!_stopwatch.IsRunning)
            return;

        _stopwatch.Stop();
        LogCompleted();

        _logScope.Dispose();
    }

    private void LogStarting()
    {
        Logger.Log(LogLevel, default, this, exception: null, StartingFormatter);
    }

    private void LogCompleted()
    {
        var duration  = _stopwatch.Elapsed;
        var exception = Exception;
        var failed    = exception is not null;

        if (failed)
            Logger.Log(ExceptionLogLevel, exception!);

        Logger.Log(LogLevel, default, this, exception: null, CompletedFormatter);
    }

    private static readonly Func<OperationScope, Exception?, string>
        StartingFormatter = FormatStarting;

    private static readonly Func<OperationScope, Exception?, string>
        CompletedFormatter = FormatCompleted;

    private static string FormatStarting(OperationScope scope, Exception? _)
        => scope.GetStartingMessage();

    private static string FormatCompleted(OperationScope scope, Exception? _)
        => scope.GetCompletedMessage();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetStartingMessage()
    {
#if NET6_0_OR_GREATER
        // .NET 6.0 introduced performant string interpolation
        return $"{Name}: Starting";
#else
        return Name + ": Starting";
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetCompletedMessage()
    {
        var seconds = _stopwatch.Elapsed.TotalSeconds;
        var notice  = Exception is null ? "" : " [EXCEPTION]";

#if NET6_0_OR_GREATER
        // .NET 6.0 introduced performant string interpolation
        return $"{Name}: Completed [{seconds:N3}s]{notice}";
#else
        return string.Format("{0}: Completed [{1:N3}s]{2}", Name, seconds, notice);
#endif
    }
}
