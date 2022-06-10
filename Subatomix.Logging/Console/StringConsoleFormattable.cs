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
///   An <see cref="IConsoleFormattable"/> implementation with
///   <see cref="string"/> content.
/// </summary>
internal readonly struct StringConsoleFormattable : IConsoleFormattable
{
    private readonly string? _content;

    /// <summary>
    ///   Initializes a new <see cref="StringConsoleFormattable"/> instance
    ///   with the specified content.
    /// </summary>
    /// <param name="content">
    ///   The content.
    /// </param>
    public StringConsoleFormattable(string? content)
    {
        _content = content;
    }

    /// <inheritdoc/>
    public bool Write(TextWriter writer, ConsoleContext console)
    {
        if (string.IsNullOrEmpty(_content))
            return false;

        writer.Write(_content);
        return true;
    }
}
