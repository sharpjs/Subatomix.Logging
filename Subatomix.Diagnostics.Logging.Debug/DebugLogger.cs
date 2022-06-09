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

using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics.Logging.Debug;

using My = Debug;

/// <summary>
///   A logger that sends messages to an attached debugger.  In Visual Studio,
///   logged messages will appear in the debug output window.
/// </summary>
public class DebugLogger : ILogger
{
    /// <summary>
    ///   Initializes a new <see cref="DebugLogger"/> instance.
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
    internal DebugLogger(DebugLoggerProvider provider, string name)
    {
        Provider = provider
            ?? throw new ArgumentNullException(nameof(provider));

        Name = name
            ?? throw new ArgumentNullException(nameof(name));

        Debugger = My.Debugger.Instance;
    }

    /// <summary>
    ///   Gets the associated provider.
    /// </summary>
    public DebugLoggerProvider Provider { get; }

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
            ?? NullDisposable.Instance;
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
            LogLevel.Trace       => "trac: ",
            LogLevel.Debug       => "dbug: ",
            LogLevel.Information => "info: ",
            LogLevel.Warning     => "warn: ",
            LogLevel.Error       => "FAIL: ",
            LogLevel.Critical    => "CRIT: ",
            _                    => "????: "
        };
    }
}
