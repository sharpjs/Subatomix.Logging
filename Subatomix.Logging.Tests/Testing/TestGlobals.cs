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

using System.Text;
using NUnit.Framework.Internal;

namespace Subatomix.Testing;

/// <summary>
///   Convenience items available globally in tests.
/// </summary>
internal static class TestGlobals
{
    /// <summary>
    ///   Gets the NUnit <see cref="Randomizer"/> for the current
    ///   <see cref="TestContext"/>.
    /// </summary>
    public static Randomizer Any => TestContext.CurrentContext.Random;

    /// <summary>
    ///   Gets a random <see cref="MEL.LogLevel"/> except
    ///   <see cref="MEL.LogLevel.None"/>.
    /// </summary>
    /// <param name="any">
    ///   A NUnit <see cref="Randomizer"/>.
    /// </param>
    public static MEL.LogLevel LogLevelExceptNone(this Randomizer any)
        => (MEL.LogLevel) any.Next(
            minValue: (int) MEL.LogLevel.Trace, // inclusive
            maxValue: (int) MEL.LogLevel.None   // exclusive
        );

    /// <summary>
    ///   Appends <see cref="Environment.NewLine"/> to the specified string.
    /// </summary>
    /// <param name="line">
    ///   The string to which to append <see cref="Environment.NewLine"/>.
    /// </param>
    public static string Lines(string line)
    {
        return line + Environment.NewLine;
    }

    /// <summary>
    ///   Concatenates the specified strings, appending
    ///   <see cref="Environment.NewLine"/> to each.
    /// </summary>
    /// <param name="lines">
    ///   The strings to concatenate.
    /// </param>
    public static string Lines(params string[] lines)
    {
        var length = 0;

        foreach (var line in lines)
            length += line.Length + Environment.NewLine.Length;

        var sb = new StringBuilder(length);

        foreach (var line in lines)
            sb.AppendLine(line);

        return sb.ToString();
    }
}
