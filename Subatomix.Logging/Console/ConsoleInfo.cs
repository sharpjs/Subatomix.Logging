// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;

namespace Subatomix.Logging.Console;

internal static class ConsoleInfo
{
    [ExcludeFromCodeCoverage]
    internal static bool IsRedirected
        => System.Console.IsOutputRedirected
        || System.Console.IsErrorRedirected;
}
