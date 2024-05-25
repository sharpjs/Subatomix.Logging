// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Console;

internal interface IPrettyConsoleFormatterTheme
{
    Styles GetStyles(LogLevel logLevel);
}
