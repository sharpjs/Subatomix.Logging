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

namespace Subatomix.Diagnostics;

using static Ansi;

internal class Styler
{
    public virtual void UseTimestampStyle (TextWriter writer) { }
    public virtual void UseTraceIdStyle   (TextWriter writer) { }
    public virtual void UseLogLevelStyle  (TextWriter writer) { }
    public virtual void UseMessageStyle   (TextWriter writer) { }
    public virtual void Reset             (TextWriter writer) { }

    public static Styler     Null        { get; } = new();
    public static AnsiStyler Verbose     { get; } = new(TV, IV, LV, MV);
    public static AnsiStyler Information { get; } = new(TI, II, LI, MI);
    public static AnsiStyler Warning     { get; } = new(TW, IW, LW, MW);
    public static AnsiStyler Error       { get; } = new(TE, IE, LE, ME);
    public static AnsiStyler Critical    { get; } = new(TC, IC, LC, MC);

    private const string
        // Timestamp
        TV = Begin + ForeBrightBlack   + And + Fore256 + "239" + End,
        TI = Begin + ForeWhite         + And + Fore256 + "242" + End,
        TW = Begin + ForeWhite         + And + Fore256 + "242" + End,
        TE = Begin + ForeWhite         + And + Fore256 + "242" + End,
        TC = Begin + ForeWhite         + And + Fore256 + "242" + End,

        // TraceId
        IV = Begin + ForeBlue          + And + Fore256 +  "23" + End,
        II = Begin + ForeCyan          + And + Fore256 +  "31" + End,
        IW = Begin + ForeCyan          + And + Fore256 +  "31" + End,
        IE = Begin + ForeCyan          + And + Fore256 +  "31" + End,
        IC = Begin + ForeCyan          + And + Fore256 +  "31" + End,

        // Level
        LV = Begin + ForeBrightBlack   + And + Fore256 + "243"          + End,
        LI = Begin + ForeDefault                                        + End,
        LW = Begin + ForeYellow                                         + End,
        LE = Begin + ForeBrightWhite   + And + BackRed     + And + Bold + End,
        LC = Begin + ForeBrightWhite   + And + BackMagenta + And + Bold + End,

        // Message
        MV = "",
        MI = "",
        MW = "",
        ME = Begin + ForeBrightRed     + And + BackDefault + End,
        MC = Begin + ForeBrightMagenta + And + BackDefault + End;
}
