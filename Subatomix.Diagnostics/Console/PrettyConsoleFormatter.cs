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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
// Rationale: Configure method sets non-nullable fields to non-null values and is invoked from the constructor.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Subatomix.Diagnostics.Internal;

namespace Subatomix.Diagnostics.Console;

using static LogLevel;

/// <summary>
///   A prettier formatter than <see cref="SimpleConsoleFormatter"/>.
/// </summary>
/// <remarks>
///   Configure this formatter via <see cref="PrettyConsoleFormatterOptions"/>.
///   Note that this formatter ignores the
///   <see cref="ConsoleFormatterOptions.IncludeScopes"/> and
///   <see cref="ConsoleFormatterOptions.TimestampFormat"/> properties.
/// </remarks>
public sealed class PrettyConsoleFormatter : ConsoleFormatter, IDisposable
{
    // Uses SimpleConsoleFormatter as an example of how to write a ConsoleFormatter.
    // https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs

    /// <summary>
    ///   The name associated with <see cref="PrettyConsoleFormatter"/>.
    /// </summary>
    public new const string Name = "pretty";

    // Active theme
    private IPrettyConsoleFormatterTheme _theme;

    // Opaque token representing subscription to options change notifications
    private readonly IDisposable _optionsChangeToken;

    /// <summary>
    ///   Initializes a new <see cref="PrettyConsoleFormatter"/> instance.
    /// </summary>
    /// <param name="options">
    ///   An accessor that provides the current options for the formatter and
    ///   notifies when those options change.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public PrettyConsoleFormatter(IOptionsMonitor<PrettyConsoleFormatterOptions> options)
        : base(Name)
    {
        if (options is null)
            throw new ArgumentNullException(nameof(options));

        IsConsoleRedirected = ConsoleInfo.IsRedirected;

        Configure(options.CurrentValue);

        _optionsChangeToken = options.OnChange(Configure);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        _optionsChangeToken.Dispose();
    }

    /// <summary>
    ///   Gets the most recently set options.
    /// </summary>
    public PrettyConsoleFormatterOptions Options { get; private set; }

    /// <summary>
    ///   Gets whether color output is enabled.
    /// </summary>
    public bool IsColorEnabled { get; private set; }

    // To enable tests to simulate console redirection
    internal bool IsConsoleRedirected { get; set; }

    // To enable tests to perform time travel
    internal IClock Clock { get; set; }

    private void Configure(PrettyConsoleFormatterOptions options)
    {
        Options = options;

        Clock = options.UseUtcTimestamp
            ? UtcClock  .Instance
            : LocalClock.Instance;

        IsColorEnabled = options.ColorBehavior switch
        {
            LoggerColorBehavior.Disabled => false,
            LoggerColorBehavior.Enabled  => true,
            _                            => !IsConsoleRedirected
        };

        _theme = IsColorEnabled
            ? new PrettyConsoleFormatterColorTheme()
            : new PrettyConsoleFormatterMonoTheme();
    }

    /// <inheritdoc/>>
    public override void Write<TState>(
        in LogEntry<TState>    entry,
        IExternalScopeProvider scopes,
        TextWriter             writer)
    {
        // Example:
        // [23:59:59] #4c9b info: This is the message.

        if (entry.State is IConsoleFormattable formattable)
        {
            Write(entry, writer, formattable);
        }
        else if (TryGetMessage(entry, out var message))
        {
            Write(entry, writer, new StringConsoleFormattable(message));
        }
        else if (entry.Exception is not null)
        {
            Write(entry, writer, new StringConsoleFormattable());
        }
    }

    private void Write<TState, TMessage>(
        in LogEntry<TState> entry,
        TextWriter          writer,
        TMessage            message)
        where TMessage : IConsoleFormattable
    {
        var styles = _theme.GetStyles(entry.LogLevel);

        WriteTimestamp(writer, styles, Clock.Now);
        WriteTraceId  (writer, styles);
        WriteLogLevel (writer, styles, entry.LogLevel);
        WriteSeparator(writer, styles);

        var wroteMessage = message.Write(writer, styles.MessageContext);

        WriteException(writer, styles, entry.Exception, wroteMessage);
        WriteEndOfLine(writer, styles);
    }

    private static void WriteTimestamp(TextWriter writer, Styles styles, DateTime now)
    {
        styles.UseTimestampStyle(writer);

        writer.Write('['); WriteTimePart(writer, now.Hour);
        writer.Write(':'); WriteTimePart(writer, now.Minute);
        writer.Write(':'); WriteTimePart(writer, now.Second);
        writer.Write(']'); 
        writer.Write(' '); 
    }

    private static void WriteTraceId(TextWriter writer, Styles styles)
    {
        if (Activity.Current is { } activity)
            WriteTraceId(writer, styles, activity);
        else
            WriteTraceIdEmpty(writer, styles);
    }

    private static void WriteTraceId(TextWriter writer, Styles styles, Activity activity)
    {
        Span<char> chars = stackalloc char[4];
        activity.GetRootOperationId(chars);

        styles.UseTraceIdStyle(writer);

        writer.Write('#');
#if NET6_0_OR_GREATER
        writer.Write(chars);
#else
        writer.Write(chars[0]);
        writer.Write(chars[1]);
        writer.Write(chars[2]);
        writer.Write(chars[3]);
#endif
        writer.Write(' ');
    }

    private static void WriteTraceIdEmpty(TextWriter writer, Styles styles)
    {
        // TODO: Use a grayed-out style here
        styles.UseTraceIdStyle(writer);

        writer.Write("..... ");
    }

    private static void WriteLogLevel(TextWriter writer, Styles styles, LogLevel logLevel)
    {
        styles.UseLogLevelStyle(writer);

        writer.Write(Format(logLevel)); 
    }

    private static void WriteSeparator(TextWriter writer, Styles styles)
    {
        styles.UseMessageStyle(writer);

        writer.Write(": ");
    }

    private static void WriteException(TextWriter writer, Styles styles,
        Exception? exception, bool wroteMessage)
    {
        if (exception is null)
            return;

        styles.UseMessageStyle(writer);

        if (wroteMessage)
            writer.Write(' ');

        writer.Write(exception.ToString());
    }

    private static void WriteEndOfLine(TextWriter writer, Styles styles)
    {
        styles.ResetStyle(writer);

        writer.WriteLine();
    }

    private static void WriteTimePart(TextWriter writer, int value)
    {
        if (value < 10)
            writer.Write('0');

        writer.Write(value);
    }

    private static bool TryGetMessage<TState>(
        in LogEntry<TState>               entry,
        [MaybeNullWhen(false)] out string message)
    {
        return !string.IsNullOrEmpty(
            message = entry.Formatter?.Invoke(entry.State, entry.Exception)
        );
    }

    private static string Format(LogLevel logLevel)
    {
        return logLevel switch
        {
            Trace       => "trce",
            Debug       => "dbug",
            Information => "info",
            Warning     => "warn",
            Error       => "FAIL",
            Critical    => "CRIT",
            _           => "    ",
        };
    }
}
