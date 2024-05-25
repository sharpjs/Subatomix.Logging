// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Console;

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
