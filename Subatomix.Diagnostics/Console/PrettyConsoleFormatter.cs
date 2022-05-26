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

using MessageWriter     = Func<TextWriter, string,              bool>;
using FormattableWriter = Func<TextWriter, IConsoleFormattable, bool>;

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

    // Writers for string messages
    private static readonly MessageWriter
        EmptyMessageWriter  = WriteEmptyMessage,
        StringMessageWriter = WriteStringMessage;

    // Writers for IConsoleFormattable objects
    private static readonly FormattableWriter
        FormattableWriterMono  = WriteFormattableMono,
        FormattableWriterColor = WriteFormattableColor;

    // Delegate that decides which styler to use
    private Func<LogLevel, Styler> _stylerSelector;

    // Active writer for IConsoleFormattable objects
    private FormattableWriter _formattableWriter;

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

        _stylerSelector = IsColorEnabled
            ? GetColorStyler
            : GetMonoStyler;

        _formattableWriter = IsColorEnabled
            ? FormattableWriterColor
            : FormattableWriterMono;
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
            Write(entry, writer, formattable, _formattableWriter);
        }
        else if (TryGetMessage(entry, out var message))
        {
            Write(entry, writer, message, StringMessageWriter);
        }
        else if (entry.Exception is not null)
        {
            Write(entry, writer, null!, EmptyMessageWriter);
        }
    }

    private static bool TryGetMessage<TState>(
        in LogEntry<TState> entry,
        [MaybeNullWhen(false)] out string message)
    {
        message = entry.Formatter?.Invoke(entry.State, entry.Exception);
        return !string.IsNullOrEmpty(message);
    }

    private void Write<TState, TMessage>(
        in LogEntry<TState>              entry,
        TextWriter                       writer,
        TMessage                         message,
        Func<TextWriter, TMessage, bool> messageWriter)
    {
        var styler = _stylerSelector(entry.LogLevel);

        WriteTimestamp(writer, styler, Clock.Now);
        WriteTraceId  (writer, styler);
        WriteLogLevel (writer, styler, entry.LogLevel);
        WriteSeparator(writer, styler);

        var wroteMessage = messageWriter(writer, message);

        WriteException(writer, styler, entry.Exception, wroteMessage);
        WriteEndOfLine(writer, styler);
    }

    private static void WriteTimestamp(TextWriter writer, Styler styler, DateTime now)
    {
        styler.UseTimestampStyle(writer);

        writer.Write('['); WriteTimePart(writer, now.Hour);
        writer.Write(':'); WriteTimePart(writer, now.Minute);
        writer.Write(':'); WriteTimePart(writer, now.Second);
        writer.Write(']'); 
        writer.Write(' '); 
    }

    private static void WriteTraceId(TextWriter writer, Styler styler)
    {
        styler.UseTraceIdStyle(writer);

        if (Activity.Current is { } activity)
        {
            var (byte0, byte1) = GetShortTraceId(activity);

            writer.Write('#');
            WriteHex(writer, byte0);
            WriteHex(writer, byte1);
            writer.Write(' ');
        }
        else
        {
            writer.Write("..... ");
        }
    }

    private static void WriteLogLevel(TextWriter writer, Styler styler, LogLevel logLevel)
    {
        styler.UseLogLevelStyle(writer);

        writer.Write(Format(logLevel)); 
    }

    private static void WriteSeparator(TextWriter writer, Styler styler)
    {
        styler.UseMessageStyle(writer);

        writer.Write(": ");
    }

    private static bool WriteEmptyMessage(TextWriter writer, string message)
    {
        return false;
    }

    private static bool WriteStringMessage(TextWriter writer, string message)
    {
        writer.Write(message);
        return true;
    }

    private static bool WriteFormattableMono(TextWriter writer, IConsoleFormattable message)
    {
        return message.Write(writer, color: false);
    }

    private static bool WriteFormattableColor(TextWriter writer, IConsoleFormattable message)
    {
        return message.Write(writer, color: true);
    }

    private static void WriteException(TextWriter writer, Styler styler,
        Exception? exception, bool wroteMessage)
    {
        if (exception is null)
            return;

        styler.UseMessageStyle(writer);

        if (wroteMessage)
            writer.Write(' ');

        writer.Write(exception.ToString());
    }

    private static void WriteEndOfLine(TextWriter writer, Styler styler)
    {
        styler.Reset(writer);

        writer.WriteLine();
    }

    private static void WriteTimePart(TextWriter writer, int value)
    {
        if (value < 10)
            writer.Write('0');

        writer.Write(value);
    }

    private static void WriteHex(TextWriter writer, byte value)
    {
        WriteHexDigit(writer, value >> 4);
        WriteHexDigit(writer, value     );
    }

    private static void WriteHexDigit(TextWriter writer, int value)
    {
        writer.Write(HexDigits[value & 0xF]);
    }

    private static (byte, byte) GetShortTraceId(Activity activity)
    {
        Span<byte> id = stackalloc byte[16];

        activity.TraceId.CopyTo(id);

        return (id[14], id[15]);
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

    private static Styler GetMonoStyler(LogLevel level)
    {
        return Styler.Null;
    }

    private static Styler GetColorStyler(LogLevel level)
    {
        return level switch
        {
            Trace       => Styler.Verbose,
            Debug       => Styler.Verbose,
            Information => Styler.Information,
            Warning     => Styler.Warning,
            Error       => Styler.Error,
            Critical    => Styler.Critical,
            _           => Styler.Verbose,
        };
    }

    private static readonly char[] HexDigits =
    {
        '0', '1', '2', '3',
        '4', '5', '6', '7',
        '8', '9', 'a', 'b',
        'c', 'd', 'e', 'f'
    };
}
