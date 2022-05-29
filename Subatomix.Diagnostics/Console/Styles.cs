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

namespace Subatomix.Diagnostics.Console;

using static Ansi;

internal class Styles
{
    private Styles() { }

    private Styles(
        string? timestampStyle,
        string? traceIdStyle,
        string? logLevelStyle,
        string? messageStyle,
        string? resetToMessageStyle)
    {
        TimestampStyle = timestampStyle;
        TraceIdStyle   = traceIdStyle;
        LogLevelStyle  = logLevelStyle;
        MessageStyle   = messageStyle;
        MessageContext = new(resetToMessageStyle);
        DefaultStyle   = timestampStyle is not null ? Begin + Reset + End : null;
        // TODO: De-uglify
    }

    public string?        TimestampStyle { get; }
    public string?        TraceIdStyle   { get; }
    public string?        LogLevelStyle  { get; }
    public string?        MessageStyle   { get; }
    public string?        DefaultStyle   { get; }

    public ConsoleContext MessageContext { get; }

    public static Styles None        { get; } = new();
    public static Styles Verbose     { get; } = new(TV, IV, LV, MV, RV);
    public static Styles Information { get; } = new(TI, II, LI, MI, RI);
    public static Styles Warning     { get; } = new(TW, IW, LW, MW, RW);
    public static Styles Error       { get; } = new(TE, IE, LE, ME, RE);
    public static Styles Critical    { get; } = new(TC, IC, LC, MC, RC);

    private const string?
        // Timestamp
        TV = Begin + Reset + And + ForeBrightBlack + And + Fore256 + "239" + End,
        TI = Begin + Reset + And + ForeWhite       + And + Fore256 + "242" + End,
        TW = TI,
        TE = TI,
        TC = TI,

        // TraceId
        IV = Begin + ForeBlue + And + Fore256 + "23" + End,
        II = Begin + ForeCyan + And + Fore256 + "31" + End,
        IW = II,
        IE = II,
        IC = II,

        // Level
        LV = Begin + ForeBrightBlack + And + Fore256 + "243"          + End,
        LI = Begin + ForeDefault                                      + End,
        LW = Begin + ForeYellow                                       + End,
        LE = Begin + ForeBrightWhite + And + BackRed     + And + Bold + End,
        LC = Begin + ForeBrightWhite + And + BackMagenta + And + Bold + End,

        // Message
        MV = null, // keep level style
        MI = null, // keep level style
        MW = null, // keep level style
        ME = Begin + ForeBrightRed     + And + BackDefault + End,
        MC = Begin + ForeBrightMagenta + And + BackDefault + End,

        // Reset + Message
        RV = Begin + Reset + And + ForeBrightBlack + And + Fore256 + "243" + End,
        RI = Begin + Reset                                                 + End,
        RW = Begin + Reset + And + ForeYellow                              + End,
        RE = Begin + Reset + And + ForeBrightRed     + And + Bold          + End,
        RC = Begin + Reset + And + ForeBrightMagenta + And + Bold          + End;

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

    #pragma warning disable IDE0060 // Unused parameter
    // Rationale: Method needs to have this signature.

    public static Styles GetMonoStyles(LogLevel logLevel)
        => None;

    public static Styles GetColorStyles(LogLevel logLevel)
        => logLevel switch
        {
            LogLevel.Information => Information,
            LogLevel.Warning     => Warning,
            LogLevel.Error       => Error,
            LogLevel.Critical    => Critical,
            _                    => Verbose,
        };
}
