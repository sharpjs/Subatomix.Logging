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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Subatomix.Logging.Internal;
using Subatomix.Logging.Testing;
using Subatomix.Testing;

namespace Subatomix.Logging.Console;

using static ExceptionTestHelpers;
using static LogLevel;
using Color = LoggerColorBehavior;

[TestFixture]
[SetCulture("kl-GL")] // Kalaallisut (Greenland), to verify nonreliance on en-US
public class PrettyConsoleFormatterTests
{
    private const string
        NullEntryFormatter = nameof(NullEntryFormatter);

    [Test]
    public void Construct_NullOptions()
    {
        Invoking(() => new PrettyConsoleFormatter(null!))
            .Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Options()
    {
        using var h = new TestHarness();

        var optionsA = h.Options.CurrentValue;
        var optionsB = new PrettyConsoleFormatterOptions();

        h.Formatter.Options.Should().BeSameAs(optionsA);

        h.Options.CurrentValue = optionsB;
        h.Options.NotifyChanged();

        h.Formatter.Options.Should().BeSameAs(optionsB);
    }

    [Test]
    public void IsColorEnabled_Default()
    {
        using var h = new TestHarness();

        //h.Formatter.IsColorEnabled can be either true or false here

        h.Formatter.IsConsoleRedirected = false;
        h.Options.NotifyChanged();
        h.Formatter.IsColorEnabled.Should().BeTrue();

        h.Formatter.IsConsoleRedirected = true;
        h.Options.NotifyChanged();
        h.Formatter.IsColorEnabled.Should().BeFalse();
    }

    [Test]
    [TestCase(Color.Disabled, false)]
    [TestCase(Color.Enabled,  true )]
    public void IsColorEnabled_Explicit(Color behavior, bool expected)
    {
        using var h = new TestHarness(o => o.ColorBehavior = behavior);

        h.Formatter.IsColorEnabled.Should().Be(expected);

        h.Formatter.IsConsoleRedirected = false;
        h.Options.NotifyChanged();
        h.Formatter.IsColorEnabled.Should().Be(expected);

        h.Formatter.IsConsoleRedirected = true;
        h.Options.NotifyChanged();
        h.Formatter.IsColorEnabled.Should().Be(expected);
    }

    [Test]
    public void Clock_Local()
    {
        using var h = new TestHarness();

        h.Formatter.Clock.Should().BeSameAs(LocalClock.Instance);
    }

    [Test]
    public void Clock_Utc()
    {
        using var h = new TestHarness(o => o.UseUtcTimestamp = true);

        h.Formatter.Clock.Should().BeSameAs(UtcClock.Instance);
    }

    public static IEnumerable<TestCaseData> WriteTestCases_All
        => Enumerable.Concat(
            WriteTestCases_Normal,
            WriteTestCases_NullStateOrFormatter
        );

    public static IEnumerable<TestCaseData> WriteTestCases_Normal => new[]
    {
        // Mono

        // Message only
        //   Level        Color  Message             Throw  Expected
        Case(None,        false, "Message.",         false, "[01:23:45] ?????     : Message."),
        Case(Trace,       false, "Message.",         false, "[01:23:45] ????? trce: Message."),
        Case(Debug,       false, "Message.",         false, "[01:23:45] ????? dbug: Message."),
        Case(Information, false, "Message.",         false, "[01:23:45] ????? info: Message."),
        Case(Warning,     false, "Message.",         false, "[01:23:45] ????? warn: Message."),
        Case(Error,       false, "Message.",         false, "[01:23:45] ????? FAIL: Message."),
        Case(Critical,    false, "Message.",         false, "[01:23:45] ????? CRIT: Message."),

        // Message and exception
        //   Level        Color  Message             Throw  Expected
        Case(None,        false, "Message.",         true,  "[01:23:45] ?????     : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Trace,       false, "Message.",         true,  "[01:23:45] ????? trce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Debug,       false, "Message.",         true,  "[01:23:45] ????? dbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Information, false, "Message.",         true,  "[01:23:45] ????? info: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Warning,     false, "Message.",         true,  "[01:23:45] ????? warn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Error,       false, "Message.",         true,  "[01:23:45] ????? FAIL: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Critical,    false, "Message.",         true,  "[01:23:45] ????? CRIT: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),

        // Exception only
        //   Level        Color  Message             Throw  Expected
        Case(None,        false, "",                 true,  "[01:23:45] ?????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Trace,       false, "",                 true,  "[01:23:45] ????? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Debug,       false, "",                 true,  "[01:23:45] ????? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Information, false, "",                 true,  "[01:23:45] ????? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Warning,     false, "",                 true,  "[01:23:45] ????? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Error,       false, "",                 true,  "[01:23:45] ????? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Critical,    false, "",                 true,  "[01:23:45] ????? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),

        // Color

        // Message only
        //   Level        Color  Message             Throw  Expected
        Case(None,        true,  "Message.",         false, "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243m    : Message.~[0m"),
        Case(Trace,       true,  "Message.",         false, "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mtrce: Message.~[0m"),
        Case(Debug,       true,  "Message.",         false, "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mdbug: Message.~[0m"),
        Case(Information, true,  "Message.",         false, "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[39minfo: Message.~[0m"),
        Case(Warning,     true,  "Message.",         false, "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[33mwarn: Message.~[0m"),
        Case(Error,       true,  "Message.",         false, "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;41;1mFAIL~[91;49m: Message.~[0m"),
        Case(Critical,    true,  "Message.",         false, "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;45;1mCRIT~[95;49m: Message.~[0m"),

        // Message and exception
        //   Level        Color  Message             Throw  Expected
        Case(None,        true,  "Message.",         true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243m    : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Trace,       true,  "Message.",         true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mtrce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Debug,       true,  "Message.",         true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mdbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Information, true,  "Message.",         true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[39minfo: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Warning,     true,  "Message.",         true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[33mwarn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Error,       true,  "Message.",         true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;41;1mFAIL~[91;49m: Message.~[91;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Critical,    true,  "Message.",         true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;45;1mCRIT~[95;49m: Message.~[95;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),

        // Exception only
        //   Level        Color  Message             Throw  Expected
        Case(None,        true,  "",                 true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Trace,       true,  "",                 true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Debug,       true,  "",                 true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Information, true,  "",                 true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Warning,     true,  "",                 true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Error,       true,  "",                 true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Critical,    true,  "",                 true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
    };

    public static IEnumerable<TestCaseData> WriteTestCases_NullStateOrFormatter => new[]
    {
        // Mono

        // Exception only (entry.State is null)
        //   Level        Color  Message             Throw  Expected
        Case(None,        false, null,               true,  "[01:23:45] ?????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Trace,       false, null,               true,  "[01:23:45] ????? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Debug,       false, null,               true,  "[01:23:45] ????? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Information, false, null,               true,  "[01:23:45] ????? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Warning,     false, null,               true,  "[01:23:45] ????? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Error,       false, null,               true,  "[01:23:45] ????? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Critical,    false, null,               true,  "[01:23:45] ????? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),

        // Exception only (entry.Formatter is null)
        //   Level        Color  Message             Throw  Expected
        Case(None,        false, NullEntryFormatter, true,  "[01:23:45] ?????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Trace,       false, NullEntryFormatter, true,  "[01:23:45] ????? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Debug,       false, NullEntryFormatter, true,  "[01:23:45] ????? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Information, false, NullEntryFormatter, true,  "[01:23:45] ????? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Warning,     false, NullEntryFormatter, true,  "[01:23:45] ????? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Error,       false, NullEntryFormatter, true,  "[01:23:45] ????? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),
        Case(Critical,    false, NullEntryFormatter, true,  "[01:23:45] ????? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*"),

        // Color
                            
        // Exception only (entry.State is null)
        //   Level        Color  Message             Throw  Expected
        Case(None,        true,  null,               true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Trace,       true,  null,               true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Debug,       true,  null,               true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Information, true,  null,               true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Warning,     true,  null,               true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Error,       true,  null,               true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Critical,    true,  null,               true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
                            
        // Exception only (entry.Formatter is null)
        //   Level        Color  Message             Throw  Expected
        Case(None,        true,  NullEntryFormatter, true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Trace,       true,  NullEntryFormatter, true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Debug,       true,  NullEntryFormatter, true,  "~[0;90;38;5;239m[01:23:45] ~[34;38;5;23m????? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Information, true,  NullEntryFormatter, true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Warning,     true,  NullEntryFormatter, true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Error,       true,  NullEntryFormatter, true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
        Case(Critical,    true,  NullEntryFormatter, true,  "~[0;37;38;5;242m[01:23:45] ~[36;38;5;31m????? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Logging.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m"),
    };

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase(NullEntryFormatter)]
    public void Write_Message_Skipped(string? message)
    {
        Write(message: message).Should().BeEmpty();
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_All))]
    public void Write_Message_NotInActivity(
        LogLevel logLevel, bool color, string? message, bool exceptional,
        params string[] expected)
    {
        var result = Write(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_All))]
    [Retry(3)] // Because TraceId #0000 has a small chance of being legitimate.
    public void Write_Message_InActivity_W3C(
        LogLevel logLevel, bool color, string? message, bool exceptional,
        params string[] expected)
    {
        using var _ = new Activity("Test")
            .SetIdFormat(ActivityIdFormat.W3C)
            .Start();

        var result = Write(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
        result.Should().MatchRegex(@"[ m]#[0-9a-z]{4} ");
        result.Should().NotContain(@"#0000");
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_All))]
    [Retry(3)] // Because TraceId #0000 has a small chance of being legitimate.
    public void Write_Message_InActivity_Hierarchical(
        LogLevel logLevel, bool color, string? message, bool exceptional,
        params string[] expected)
    {
        using var _ = new Activity("Test")
            .SetIdFormat(ActivityIdFormat.Hierarchical)
            .Start();

        var result = Write(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
        result.Should().MatchRegex(@"[ m]#[0-9a-z]{4} ");
        result.Should().NotContain(@"#0000");
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_Normal))]
    public void Write_Formattable_NotInActivity(
        LogLevel logLevel, bool color, string message, bool exceptional,
        params string[] expected)
    {
        var result = WriteFormattable(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_Normal))]
    [Retry(3)] // Because TraceId #0000 has a small chance of being legitimate.
    public void Write_Formattable_InActivity_W3C(
        LogLevel logLevel, bool color, string message, bool exceptional,
        params string[] expected)
    {
        using var _ = new Activity("Test")
            .SetIdFormat(ActivityIdFormat.W3C)
            .Start();

        var result = WriteFormattable(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
        result.Should().MatchRegex(@"[ m]#[0-9a-z]{4} ");
        result.Should().NotContain(@"#0000");
    }

    [Test]
    [TestCaseSource(nameof(WriteTestCases_Normal))]
    [Retry(3)] // Because TraceId #0000 has a small chance of being legitimate.
    public void Write_Formattable_InActivity_Hierarchical(
        LogLevel logLevel, bool color, string message, bool exceptional,
        params string[] expected)
    {
        using var _ = new Activity("Test")
            .SetIdFormat(ActivityIdFormat.Hierarchical)
            .Start();

        var result = WriteFormattable(logLevel, color, message, exceptional);

        result.Should().Match(Lines(expected));
        result.Should().MatchRegex(@"[ m]#[0-9a-z]{4} ");
        result.Should().NotContain(@"#0000");
    }

    public static TestCaseData Case(
        LogLevel        logLevel,
        bool            color,
        string?         content,
        bool            exceptional,
        params string[] expected)
    {
        return new(logLevel, color, content, exceptional, expected);
    }

    private static string Write(
        LogLevel logLevel    = Information,
        bool     color       = false,
        string?  message     = "Message.",
        bool     exceptional = false)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var exception = exceptional ? Thrown() : null;
        var entry     = h.CreateEntry(logLevel, message, exception);

        return h.Write(entry);
    }

    private static string WriteFormattable(
        LogLevel logLevel    = Information,
        bool     color       = false,
        string   content     = "Formattable.",
        bool     exceptional = false)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var state     = h.CreateFormattable(content, color);
        var exception = exceptional ? Thrown() : null;
        var entry     = h.CreateEntry(logLevel, state, exception);

        return h.Write(entry);
    }

    private class TestHarness : TestHarnessBase
    {
        public TestOptionsMonitor<PrettyConsoleFormatterOptions> Options { get; }

        public IExternalScopeProvider Scopes { get; }

        public PrettyConsoleFormatter Formatter { get; }

        public TestHarness(bool colors)
            : this(o => o.ColorBehavior = colors ? Color.Enabled : Color.Disabled)
        { }

        public TestHarness(Action<PrettyConsoleFormatterOptions>? configure = null)
        {
            Options = new(Mocks);
            configure?.Invoke(Options.CurrentValue);

            Scopes = Mocks.Create<IExternalScopeProvider>().Object;

            Formatter = new PrettyConsoleFormatter(Options);
        }

        public TestHarness AtConstantTime()
        {
            Formatter.Clock = new TestClock { Now = new(2022, 5, 24, 1, 23, 45, 678) };
            return this;
        }

        public LogEntry<string?> CreateEntry(
            LogLevel logLevel, string? message, Exception? exception)
        {
            var category  = Random.GetString(); // to prove it is not used
            var eventId   = Random.Next();      // to prove it is not used
            var formatter = CreateSubformatter(message, exception);

            return new(logLevel, category, eventId, message, exception, formatter!);
        }

        public LogEntry<object> CreateEntry(
            LogLevel logLevel, IConsoleFormattable custom, Exception? exception)
        {
            var state     = (object) custom;
            var category  = Random.GetString(); // to prove it is not used
            var eventId   = Random.Next();      // to prove it is not used

            return new(logLevel, category, eventId, state, exception, ThrowingFormatter);
        }

        private static Func<string?, Exception?, string>?
            CreateSubformatter(string? message, Exception? exception)
        {
            if (message == NullEntryFormatter)
                return null;

            return (s, e) =>
            {
                s.Should().Be(message);
                e.Should().Be(exception);
                return message!; // Need to test null return, even if type does not allow it
            };
        }

        [DoesNotReturn]
        private static string ThrowingFormatter(object state, Exception? exception)
        {
            throw new AssertionException("This test should not invoke the entry's formatter.");
        }

        public IConsoleFormattable CreateFormattable(string content, bool color = false)
        {
            var formattable = Mocks.Create<IConsoleFormattable>();

            void Write(TextWriter writer, ConsoleContext console)
            {
                console.IsColorEnabled.Should().Be(color);
                writer.Write(content);
            }

            var result = !string.IsNullOrEmpty(content);

            formattable
                .Setup(o => o.Write(
                    It.IsNotNull<TextWriter>(),
                    It.IsAny<ConsoleContext>()
                ))
                .Callback(Write)
                .Returns(result)
                .Verifiable();

            return formattable.Object;
        }

        public string Write<TState>(in LogEntry<TState> entry)
        {
            using var writer = new StringWriter();

            Formatter.Write(entry, Scopes, writer);

            return writer.ToString().Replace('\x1B', '~');
        }

        protected override void Verify()
        {
            ((IDisposable) Formatter).Dispose();
            base.Verify();
        }
    }
}
