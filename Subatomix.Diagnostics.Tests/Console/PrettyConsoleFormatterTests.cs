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
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Subatomix.Diagnostics.Internal;
using Subatomix.Diagnostics.Testing;
using Subatomix.Testing;

namespace Subatomix.Diagnostics.Console;

using Color = LoggerColorBehavior;
using static LogLevel;

[TestFixture]
[SetCulture("kl-GL")] // Kalaallisut (Greenland), to verify nonreliance on en-US
public class PrettyConsoleFormatterTests
{
    private const string
        UseNullSubformatter = nameof(UseNullSubformatter);

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

    [Test]
    //
    [TestCase(None,        false, "Message.",          false, "[01:23:45] .....     : Message.")]
    [TestCase(Trace,       false, "Message.",          false, "[01:23:45] ..... trce: Message.")]
    [TestCase(Debug,       false, "Message.",          false, "[01:23:45] ..... dbug: Message.")]
    [TestCase(Information, false, "Message.",          false, "[01:23:45] ..... info: Message.")]
    [TestCase(Warning,     false, "Message.",          false, "[01:23:45] ..... warn: Message.")]
    [TestCase(Error,       false, "Message.",          false, "[01:23:45] ..... FAIL: Message.")]
    [TestCase(Critical,    false, "Message.",          false, "[01:23:45] ..... CRIT: Message.")]
    //
    [TestCase(None,        false, "Message.",          true,  "[01:23:45] .....     : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, "Message.",          true,  "[01:23:45] ..... trce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, "Message.",          true,  "[01:23:45] ..... dbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, "Message.",          true,  "[01:23:45] ..... info: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, "Message.",          true,  "[01:23:45] ..... warn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, "Message.",          true,  "[01:23:45] ..... FAIL: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, "Message.",          true,  "[01:23:45] ..... CRIT: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, "",                  true,  "[01:23:45] .....     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, "",                  true,  "[01:23:45] ..... trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, "",                  true,  "[01:23:45] ..... dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, "",                  true,  "[01:23:45] ..... info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, "",                  true,  "[01:23:45] ..... warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, "",                  true,  "[01:23:45] ..... FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, "",                  true,  "[01:23:45] ..... CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, null,                true,  "[01:23:45] .....     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, null,                true,  "[01:23:45] ..... trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, null,                true,  "[01:23:45] ..... dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, null,                true,  "[01:23:45] ..... info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, null,                true,  "[01:23:45] ..... warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, null,                true,  "[01:23:45] ..... FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, null,                true,  "[01:23:45] ..... CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, UseNullSubformatter, true,  "[01:23:45] .....     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, UseNullSubformatter, true,  "[01:23:45] ..... trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, UseNullSubformatter, true,  "[01:23:45] ..... dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, UseNullSubformatter, true,  "[01:23:45] ..... info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, UseNullSubformatter, true,  "[01:23:45] ..... warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, UseNullSubformatter, true,  "[01:23:45] ..... FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, UseNullSubformatter, true,  "[01:23:45] ..... CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : Message.~[0m")]
    [TestCase(Trace,       true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: Message.~[0m")]
    [TestCase(Debug,       true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: Message.~[0m")]
    [TestCase(Information, true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: Message.~[0m")]
    [TestCase(Warning,     true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: Message.~[0m")]
    [TestCase(Error,       true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: Message.~[0m")]
    [TestCase(Critical,    true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: Message.~[0m")]
    //
    [TestCase(None,        true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: Message.~[91;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: Message.~[95;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //
    [TestCase(None,        true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //                            
    [TestCase(None,        true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //                            
    [TestCase(None,        true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    public void Write_Message(LogLevel logLevel, bool color, string? message, bool thrown, params string[] expected)
    {
        var exception = thrown ? ExceptionTestHelpers.Thrown() : null;

        Write(logLevel, color, message, exception).Should().Match(Lines(expected));
    }

    [Test]
    //
    [TestCase(None,        false, "Message.",          false, "[01:23:45] #????     : Message.")]
    [TestCase(Trace,       false, "Message.",          false, "[01:23:45] #???? trce: Message.")]
    [TestCase(Debug,       false, "Message.",          false, "[01:23:45] #???? dbug: Message.")]
    [TestCase(Information, false, "Message.",          false, "[01:23:45] #???? info: Message.")]
    [TestCase(Warning,     false, "Message.",          false, "[01:23:45] #???? warn: Message.")]
    [TestCase(Error,       false, "Message.",          false, "[01:23:45] #???? FAIL: Message.")]
    [TestCase(Critical,    false, "Message.",          false, "[01:23:45] #???? CRIT: Message.")]
    //
    [TestCase(None,        false, "Message.",          true,  "[01:23:45] #????     : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, "Message.",          true,  "[01:23:45] #???? trce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, "Message.",          true,  "[01:23:45] #???? dbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, "Message.",          true,  "[01:23:45] #???? info: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, "Message.",          true,  "[01:23:45] #???? warn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, "Message.",          true,  "[01:23:45] #???? FAIL: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, "Message.",          true,  "[01:23:45] #???? CRIT: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, "",                  true,  "[01:23:45] #????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, "",                  true,  "[01:23:45] #???? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, "",                  true,  "[01:23:45] #???? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, "",                  true,  "[01:23:45] #???? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, "",                  true,  "[01:23:45] #???? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, "",                  true,  "[01:23:45] #???? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, "",                  true,  "[01:23:45] #???? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, null,                true,  "[01:23:45] #????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, null,                true,  "[01:23:45] #???? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, null,                true,  "[01:23:45] #???? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, null,                true,  "[01:23:45] #???? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, null,                true,  "[01:23:45] #???? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, null,                true,  "[01:23:45] #???? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, null,                true,  "[01:23:45] #???? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        false, UseNullSubformatter, true,  "[01:23:45] #????     : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Trace,       false, UseNullSubformatter, true,  "[01:23:45] #???? trce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Debug,       false, UseNullSubformatter, true,  "[01:23:45] #???? dbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Information, false, UseNullSubformatter, true,  "[01:23:45] #???? info: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Warning,     false, UseNullSubformatter, true,  "[01:23:45] #???? warn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Error,       false, UseNullSubformatter, true,  "[01:23:45] #???? FAIL: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    [TestCase(Critical,    false, UseNullSubformatter, true,  "[01:23:45] #???? CRIT: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*")]
    //
    [TestCase(None,        true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : Message.~[0m")]
    [TestCase(Trace,       true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: Message.~[0m")]
    [TestCase(Debug,       true,  "Message.",          false, "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: Message.~[0m")]
    [TestCase(Information, true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: Message.~[0m")]
    [TestCase(Warning,     true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: Message.~[0m")]
    [TestCase(Error,       true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: Message.~[0m")]
    [TestCase(Critical,    true,  "Message.",          false, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: Message.~[0m")]
    //
    [TestCase(None,        true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  "Message.",          true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: Message. System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: Message.~[91;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  "Message.",          true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: Message.~[95;49m System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //
    [TestCase(None,        true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  "",                  true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  "",                  true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //                            
    [TestCase(None,        true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  null,                true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  null,                true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    //                            
    [TestCase(None,        true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Trace,       true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Debug,       true,  UseNullSubformatter, true,  "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Information, true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Warning,     true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: System.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Error,       true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: ~[91;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    [TestCase(Critical,    true,  UseNullSubformatter, true,  "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: ~[95;49mSystem.ApplicationException: A test exception was thrown.", "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*~[0m")]
    public void Write_Message_InActivity_W3C(LogLevel logLevel, bool color, string? message, bool thrown, params string[] expected)
    {
        using var _ = new Activity("Test").SetIdFormat(ActivityIdFormat.W3C).Start();

        var exception = thrown ? ExceptionTestHelpers.Thrown() : null;

        Write(logLevel, color, message, exception).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase(UseNullSubformatter)]
    public void Write_EmptyMessage(string? message)
    {
        Write(message: message).Should().BeEmpty();
    }

    [Test]
    [TestCase(None,        "[01:23:45] .....     : Formattable.")]
    [TestCase(Trace,       "[01:23:45] ..... trce: Formattable.")]
    [TestCase(Debug,       "[01:23:45] ..... dbug: Formattable.")]
    [TestCase(Information, "[01:23:45] ..... info: Formattable.")]
    [TestCase(Warning,     "[01:23:45] ..... warn: Formattable.")]
    [TestCase(Error,       "[01:23:45] ..... FAIL: Formattable.")]
    [TestCase(Critical,    "[01:23:45] ..... CRIT: Formattable.")]
    public void Write_Formattable_Mono(LogLevel logLevel, string expected)
    {
        WriteFormattable(logLevel).Should().Be(Lines(expected));
    }

    [Test]
    [TestCase(None,        "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : Formattable.~[0m")]
    [TestCase(Trace,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: Formattable.~[0m")]
    [TestCase(Debug,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: Formattable.~[0m")]
    [TestCase(Information, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: Formattable.~[0m")]
    [TestCase(Warning,     "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: Formattable.~[0m")]
    [TestCase(Error,       "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: Formattable.~[0m")]
    [TestCase(Critical,    "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: Formattable.~[0m")]
    public void Write_Formattable_Color(LogLevel logLevel, string expected)
    {
        WriteFormattable(logLevel, color: true).Should().Be(Lines(expected));
    }

    private string Write(
        LogLevel   logLevel  = Information,
        bool       color     = false,
        string?    message   = "Message.",
        Exception? exception = null)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var entry = h.CreateEntry(logLevel, message, exception);

        return h.Write(entry);
    }

    private string WriteFormattable(
        LogLevel   logLevel  = Information,
        bool       color     = false,
        string     content   = "Formattable.",
        Exception? exception = null)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var state = h.CreateFormattable(content, color);
        var entry = h.CreateEntry(logLevel, state, exception);

        return h.Write(entry);
    }

    private static string Lines(string line)
    {
        return line + Environment.NewLine;
    }

    private static string Lines(params string[] lines)
    {
        var length = 0;

        foreach (var line in lines)
            length += line.Length + Environment.NewLine.Length;

        var sb = new StringBuilder(length);

        foreach (var line in lines)
            sb.AppendLine(line);

        return sb.ToString();
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

        public Func<string?, Exception?, string>?
            CreateSubformatter(string? message, Exception? exception)
        {
            if (message == UseNullSubformatter)
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

        public IConsoleFormattable CreateFormattable(string content, bool expectedColor = false)
        {
            var formattable = Mocks.Create<IConsoleFormattable>();

            void Write(TextWriter writer, bool color)
            {
                color.Should().Be(expectedColor);
                writer.Write(content);
            }

            var result = !string.IsNullOrEmpty(content);

            formattable
                .Setup(o => o.Write(
                    It.IsNotNull<TextWriter>(),
                    It.IsAny<bool>()
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
