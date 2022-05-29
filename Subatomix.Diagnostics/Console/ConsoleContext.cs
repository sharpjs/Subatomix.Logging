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

using System.Diagnostics.CodeAnalysis;

namespace Subatomix.Diagnostics.Console;

/// <summary>
///   Contextual information provided when formatting for console output.
/// </summary>
public readonly struct ConsoleContext
{
    /// <summary>
    ///   Initializes a new <see cref="ConsoleContext"/> instance with the
    ///   specified ANSI escape code sequence for default color and style.
    /// </summary>
    /// <param name="defaultCode">
    ///   An ANSI escape code sequence that resets the current color and style
    ///   to the default for the context, or <see langword="null"/> to disable
    ///   colorization and styling.
    /// </param>
    public ConsoleContext(string? defaultCode)
    {
        DefaultCode = defaultCode;
    }

    /// <summary>
    ///   Gets whether to use ANSI escape code sequences to control the color
    ///   and style of output.
    /// </summary>
    public bool IsColorEnabled => DefaultCode != null;

    /// <summary>
    ///   Gets an ANSI escape code sequence that resets the current color and
    ///   style to the default for the context.
    /// </summary>
    public string? DefaultCode { get; }
}
