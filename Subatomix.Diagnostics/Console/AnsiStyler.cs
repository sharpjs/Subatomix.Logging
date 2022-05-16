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

internal class AnsiStyler : Styler
{
    public AnsiStyler(
        string timestampStyle,
        string traceIdStyle,
        string logLevelStyle,
        string messageStyle)
    {
        TimestampStyle = timestampStyle;
        TraceIdStyle   = traceIdStyle;
        LogLevelStyle  = logLevelStyle;
        MessageStyle   = messageStyle;
    }

    public string TimestampStyle { get; }

    public string TraceIdStyle { get; }

    public string LogLevelStyle { get; }

    public string MessageStyle { get; }

    private const string
        ResetStyle = Ansi.Begin + Ansi.Reset + Ansi.End;

    public override void UseTimestampStyle(TextWriter writer)
        => writer.Write(TimestampStyle);

    public override void UseTraceIdStyle(TextWriter writer)
        => writer.Write(TraceIdStyle);

    public override void UseLogLevelStyle(TextWriter writer)
        => writer.Write(LogLevelStyle);

    public override void UseMessageStyle(TextWriter writer)
        => writer.Write(MessageStyle);

    public override void Reset(TextWriter writer)
        => writer.Write(ResetStyle);
}
