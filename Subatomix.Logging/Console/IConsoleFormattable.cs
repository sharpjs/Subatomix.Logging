// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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
