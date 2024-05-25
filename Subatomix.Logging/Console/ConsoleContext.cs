// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Console;

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
