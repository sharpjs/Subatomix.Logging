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

namespace Subatomix.Logging.Sql;

[TestFixture]
public class ConversionExtensionsTests
{
    [Test]
    [TestCase(LogLevel.Trace,     0)]
    [TestCase(LogLevel.Critical,  5)]
    [TestCase((LogLevel) 42,     42)]
    public void LogLevel_ToByte(LogLevel logLevel, byte expected)
    {
        logLevel.ToByte().Should().Be(expected);
    }

    [Test]
    [TestCase("",     "" )]
    [TestCase("abc",  "abc")]
    [TestCase("abcd", "abc")]
    public void String_Truncate(string input, string expected)
    {
        input.Truncate(3).Should().Be(expected);
    }
}
