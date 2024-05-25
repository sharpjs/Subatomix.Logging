// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Console;

internal class PrettyConsoleFormatterMonoTheme : IPrettyConsoleFormatterTheme
{
    public Styles Styles { get; } = new();

    public Styles GetStyles(LogLevel logLevel)
    {
        return Styles;
    }
}
