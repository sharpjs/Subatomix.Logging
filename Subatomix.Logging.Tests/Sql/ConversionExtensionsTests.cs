// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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
