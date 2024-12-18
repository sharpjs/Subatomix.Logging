// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Console;

namespace Subatomix.Logging;

/// <summary>
///   A scope representing a logical operation.  This type is intended for use
///   with the <see cref="ILogger"/> API.
/// </summary>
/// <remarks>
///   <para>
///     On construction, a scope of this type logs a start message containing
///     the name of the operation.  On disposal, a scope logs a completion
///     message containing the name and duration of the operation.
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
public class OperationScope : IConsoleFormattable, IDisposable
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
    ///   Gets whether the operation has completed.
    /// </summary>
    /// <remarks>
    ///   The value of this property is <see langword="false"/> until the
    ///   operation is disposed.  On disposal, the value transitions to
    ///   <see langword="true"/>.
    /// </remarks>
    public bool IsCompleted { get; private set; }

    /// <summary>
    ///   Gets the duration of the operation.
    /// </summary>
    /// <remarks>
    ///   The value of this property steadily increases until the operation
    ///   is disposed.  After disposal, the value remains constant.
    /// </remarks>
    public TimeSpan Duration => _stopwatch.Elapsed;

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
    ///   <see cref="MEL.LogLevel.Error"/>.
    /// </summary>
    public LogLevel ExceptionLogLevel { get; set; } = LogLevel.Error;

    // Opaque scope returned by ILogger.BeginScope().
    private readonly IDisposable? _logScope;

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
        if (!IsCompleted)
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
        _stopwatch.Stop();

        IsCompleted = true;
        LogCompleted();

        _logScope?.Dispose();
    }

    private void LogStarting()
    {
        Logger.Log(LogLevel, default, this, exception: null, StartingFormatter);
    }

    private void LogCompleted()
    {
        if (Exception is { } exception)
            Logger.Log(ExceptionLogLevel, exception);

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

    /// <inheritdoc/>
    bool IConsoleFormattable.Write(TextWriter writer, ConsoleContext console)
    {
        return !IsCompleted
            ? WriteStartingMessage (writer, console)
            : WriteCompletedMessage(writer, console);
    }

    private bool WriteStartingMessage(TextWriter writer, ConsoleContext console)
    {
        WriteNameAndStatus(writer, console, status: "Starting");
        return true;
    }

    private bool WriteCompletedMessage(TextWriter writer, ConsoleContext console)
    {
        WriteNameAndStatus  (writer, console, "Completed");
        WriteElapsedTime    (writer, console);
        WriteExceptionNotice(writer, console);
        return true;
    }

    private void WriteNameAndStatus(TextWriter writer, ConsoleContext console, string status)
    {
        const string NameStyle
            = Ansi.Begin + Ansi.Bold
            + Ansi.End;

        const string StatusStyle
            = Ansi.Begin + Ansi.ForeBrightCyan
            + Ansi.And   + Ansi.Normal
            + Ansi.End;

        if (console.IsColorEnabled)
            writer.Write(NameStyle);

        writer.Write(Name);
        writer.Write(": ");

        if (console.IsColorEnabled)
            writer.Write(StatusStyle);

        writer.Write(status);
    }

    private void WriteElapsedTime(TextWriter writer, ConsoleContext console)
    {
        const string TimeStyle
            = Ansi.Begin + Ansi.ForeWhite
            + Ansi.And   + Ansi.Fore256 + "248"
            + Ansi.End;

        var seconds = _stopwatch.Elapsed.TotalSeconds;

        if (console.IsColorEnabled)
            writer.Write(TimeStyle);

#if NET6_0_OR_GREATER
        // .NET 6.0 introduced performant string interpolation
        writer.Write($" [{seconds:N3}s]");
#else
        writer.Write(" [{0:N3}s]", seconds);
#endif
    }

    private void WriteExceptionNotice(TextWriter writer, ConsoleContext console)
    {
        const string NoticeStyle
            = Ansi.Begin + Ansi.Bold
            + Ansi.And   + Ansi.ForeYellow
            + Ansi.End;

        if (Exception is { } exception)
        {
            if (console.IsColorEnabled)
                writer.Write(NoticeStyle);

            writer.Write(" [EXCEPTION]");
        }
    }
}
