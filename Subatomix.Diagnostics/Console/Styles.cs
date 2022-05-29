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

namespace Subatomix.Diagnostics.Console;

using static Ansi;

internal class Styles
{
    // Mono
    public Styles() { }

    // Color
    public Styles(
        string? timestampStyle,
        string? traceIdStyle,
        string? logLevelStyle,
        string? messageStyle,
        string? resetToMessageStyle)
    {
        DefaultStyle   = Begin + Reset + End;
        TimestampStyle = timestampStyle;
        TraceIdStyle   = traceIdStyle;
        LogLevelStyle  = logLevelStyle;
        MessageStyle   = messageStyle;
        MessageContext = new(resetToMessageStyle);
    }

    public string? DefaultStyle   { get; }
    public string? TimestampStyle { get; }
    public string? TraceIdStyle   { get; }
    public string? LogLevelStyle  { get; }
    public string? MessageStyle   { get; }

    public ConsoleContext MessageContext { get; }

    public void UseTimestampStyle(TextWriter writer)
        => writer.Write(TimestampStyle);

    public void UseTraceIdStyle(TextWriter writer)
        => writer.Write(TraceIdStyle);

    public void UseLogLevelStyle(TextWriter writer)
        => writer.Write(LogLevelStyle);

    public void UseMessageStyle(TextWriter writer)
        => writer.Write(MessageStyle);

    public void ResetStyle(TextWriter writer)
        => writer.Write(DefaultStyle);
}
