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

internal static class Ansi
{
    // References:
    // https://en.wikipedia.org/wiki/ANSI_escape_code#SGR_(Select_Graphic_Rendition)_parameters
    // https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#colors--graphics-mode

    internal const string
    //  Punctuation
        Begin = "\x1B[",
        And   =     ";",
        End   =     "m",

    //  Styles
    //  Set                          Clear
    //  ---------------------------  ---------------------------
                                     Reset             =  "0",      // all
        Bold              =  "1",                                   // bold or bright
        Dim               =  "2",    Normal            = "22",      // thin or dimmed
        Italic            =  "3",    NoItalic          = "23",      // italic
        Underline         =  "4",    NoUnderline       = "24",      // underline
        Blink             =  "5",    NoBlink           = "25",      // blink
        Reverse           =  "7",    NoReverse         = "27",      // reverse
        Hidden            =  "8",    NoHidden          = "28",      // hidden
        Strike            =  "9",    NoStrike          = "29",      // strikethrough

    //  Colors
    //  Foreground                   Background
    //  ---------------------------  ---------------------------
        ForeBlack         = "30",    BackBlack         = "40",      // black
        ForeRed           = "31",    BackRed           = "41",      // red
        ForeGreen         = "32",    BackGreen         = "42",      // green
        ForeYellow        = "33",    BackYellow        = "43",      // yellow
        ForeBlue          = "34",    BackBlue          = "44",      // blue
        ForeMagenta       = "35",    BackMagenta       = "45",      // magenta
        ForeCyan          = "36",    BackCyan          = "46",      // cyan
        ForeWhite         = "37",    BackWhite         = "47",      // white

        ForeBrightBlack   = "90",    BackBrightBlack   = "100",     // bright black
        ForeBrightRed     = "91",    BackBrightRed     = "101",     // bright red
        ForeBrightGreen   = "92",    BackBrightGreen   = "102",     // bright green
        ForeBrightYellow  = "93",    BackBrightYellow  = "103",     // bright yellow
        ForeBrightBlue    = "94",    BackBrightBlue    = "104",     // bright blue
        ForeBrightMagenta = "95",    BackBrightMagenta = "105",     // bright magenta
        ForeBrightCyan    = "96",    BackBrightCyan    = "106",     // bright cyan
        ForeBrightWhite   = "97",    BackBrightWhite   = "107",     // bright white

        Fore256           = "38;5;", Back256           = "48;5;",   // then ⟨n⟩         for 256 colors
        ForeRgb           = "38;2;", BackRgb           = "48;2;",   // then ⟨r⟩;⟨g⟩;⟨b⟩ for 24-bit color

        ForeDefault       = "39",    BackDefault       = "49";      // (default)
}
