// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Internal;
using SLD = Subatomix.Logging.Debugger;

namespace Subatomix.Logging.Debugger;

/// <summary>
///   A logger that sends messages to an attached debugger.  In Visual Studio,
///   logged messages will appear in the debug output window.
/// </summary>
public class DebuggerLogger : ILogger
{
    /// <summary>
    ///   Initializes a new <see cref="DebuggerLogger"/> instance.
    /// </summary>
    /// <param name="provider">
    ///   The provider that created the logger.
    /// </param>
    /// <param name="name">
    ///   The category name.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="provider"/> and/or <paramref name="name"/> is
    ///   <see langword="null"/>.
    /// </exception>
    internal DebuggerLogger(DebuggerLoggerProvider provider, string name)
    {
        Provider = provider
            ?? throw new ArgumentNullException(nameof(provider));

        Name = name
            ?? throw new ArgumentNullException(nameof(name));

        Debugger = SLD.Debugger.Instance;
    }

    /// <summary>
    ///   Gets the associated provider.
    /// </summary>
    public DebuggerLoggerProvider Provider { get; }

    /// <summary>
    ///   Gets the category name.
    /// </summary>
    public string Name { get; }

    // For testing
    internal IDebugger Debugger { get; set; }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state)
    {
        return Provider.ScopeProvider?.Push(state)
            ?? NullScope.Instance;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return Debugger.IsAttached
            && logLevel != LogLevel.None;
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel                         logLevel,
        EventId                          eventId,
        TState                           state,
        Exception?                       exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var prefix  = Format(logLevel);
        var message = formatter?.Invoke(state, exception);

        if (!string.IsNullOrEmpty(message))
        {
            message = string.Concat(prefix, message, Environment.NewLine);
            Debugger.Log((int) logLevel, Name, message);
        }

        if (exception is not null)
        {
            message = string.Concat(prefix, exception.ToString(), Environment.NewLine);
            Debugger.Log((int) logLevel, Name, message);
        }
    }

    private static string Format(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace       => "• trac: ",
            LogLevel.Debug       => "• dbug: ",
            LogLevel.Information => "• info: ",
            LogLevel.Warning     => "• warn: ",
            LogLevel.Error       => "• FAIL: ",
            LogLevel.Critical    => "• CRIT: ",
            _                    => "• ????: "
        };
    }
}
