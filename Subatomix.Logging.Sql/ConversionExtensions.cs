// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Sql;

internal static class ConversionExtensions
{
    public static byte ToByte(this LogLevel logLevel)
        => (byte) logLevel;

    public static string Truncate(this string s, int length)
        => s.Length <= length ? s : s.Substring(0, length);
}
