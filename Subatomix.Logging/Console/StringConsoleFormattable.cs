// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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
