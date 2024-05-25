// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.Extensions.Logging.Console;

namespace Subatomix.Logging.Console;

/// <summary>
///   Options for <see cref="PrettyConsoleFormatter"/>.
/// </summary>
public class PrettyConsoleFormatterOptions : ConsoleFormatterOptions
{
    /// <summary>
    ///  Gets or sets whether to use color in log messages.
    /// </summary>
    public LoggerColorBehavior ColorBehavior { get; set; }
}
