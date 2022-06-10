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

namespace Subatomix.Logging.Console;

/// <summary>
///   Exposes an object's ability to format itself for console output.
/// </summary>
public interface IConsoleFormattable
{
    /// <summary>
    ///   Formats the object for console output.
    /// </summary>
    /// <param name="writer">
    ///   The writer to which to write the formatted output.
    /// </param>
    /// <param name="console">
    ///   Contextual information, including whether to use ANSI escape code
    ///   sequences to control the color and style of output.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if one or more characters were written;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    bool Write(TextWriter writer, ConsoleContext console);
}
