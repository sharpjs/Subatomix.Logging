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
    [TestCase(UseNullSubformatter)]
    [TestCase(null)]
    [TestCase("")]
    public void Write_Message_None(string? message)
    {
        Write(message: message).Should().BeEmpty();
    }

    [Test]
    [TestCase(LogLevel.None,        "[01:23:45] .....     : Message.")]
    [TestCase(LogLevel.Trace,       "[01:23:45] ..... trce: Message.")]
    [TestCase(LogLevel.Debug,       "[01:23:45] ..... dbug: Message.")]
    [TestCase(LogLevel.Information, "[01:23:45] ..... info: Message.")]
    [TestCase(LogLevel.Warning,     "[01:23:45] ..... warn: Message.")]
    [TestCase(LogLevel.Error,       "[01:23:45] ..... FAIL: Message.")]
    [TestCase(LogLevel.Critical,    "[01:23:45] ..... CRIT: Message.")]
    public void Write_Message_Mono(LogLevel logLevel, string expected)
    {
        Write(logLevel).Should().Be(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "[01:23:45] #????     : Message.")]
    [TestCase(LogLevel.Trace,       "[01:23:45] #???? trce: Message.")]
    [TestCase(LogLevel.Debug,       "[01:23:45] #???? dbug: Message.")]
    [TestCase(LogLevel.Information, "[01:23:45] #???? info: Message.")]
    [TestCase(LogLevel.Warning,     "[01:23:45] #???? warn: Message.")]
    [TestCase(LogLevel.Error,       "[01:23:45] #???? FAIL: Message.")]
    [TestCase(LogLevel.Critical,    "[01:23:45] #???? CRIT: Message.")]
    public void Write_Message_Mono_InActivity(LogLevel logLevel, string expected)
    {
        using var _ = new Activity("Test").SetIdFormat(ActivityIdFormat.W3C).Start();

        Write(logLevel).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : Message.~[0m")]
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: Message.~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: Message.~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: Message.~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: Message.~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: Message.~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: Message.~[0m")]
    public void Write_Message_Color(LogLevel logLevel, string expected)
    {
        Write(logLevel, color: true).Should().Be(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243m    : Message.~[0m")]
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mtrce: Message.~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m#???? ~[90;38;5;243mdbug: Message.~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[39minfo: Message.~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[33mwarn: Message.~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;41;1mFAIL~[91;49m: Message.~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[01:23:45] ~[36;38;5;31m#???? ~[97;45;1mCRIT~[95;49m: Message.~[0m")]
    public void Write_Message_Color_InActivity(LogLevel logLevel, string expected)
    {
        using var _ = new Activity("Test").SetIdFormat(ActivityIdFormat.W3C).Start();

        Write(logLevel, color: true).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase(UseNullSubformatter)]
    [TestCase(null)]
    [TestCase("")]
    public void Write_Exception(string? message)
    {
        var e = ExceptionTestHelpers.Thrown();

        Write(message: message, exception: e).Should().Match(Lines(
            "[01:23:45] ..... info: System.ApplicationException: A test exception was thrown.",
            "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*"
        ));
    }

    [Test]
    public void Write_MessageAndException()
    {
        var e = ExceptionTestHelpers.Thrown();

        Write(exception: e).Should().Match(Lines(
            "[01:23:45] ..... info: Message. System.ApplicationException: A test exception was thrown.",
            "   at Subatomix.Diagnostics.Testing.ExceptionTestHelpers.Thrown(String message)*"
        ));
    }

    [Test]
    [TestCase(LogLevel.None,        "[01:23:45] .....     : Formattable.")]
    [TestCase(LogLevel.Trace,       "[01:23:45] ..... trce: Formattable.")]
    [TestCase(LogLevel.Debug,       "[01:23:45] ..... dbug: Formattable.")]
    [TestCase(LogLevel.Information, "[01:23:45] ..... info: Formattable.")]
    [TestCase(LogLevel.Warning,     "[01:23:45] ..... warn: Formattable.")]
    [TestCase(LogLevel.Error,       "[01:23:45] ..... FAIL: Formattable.")]
    [TestCase(LogLevel.Critical,    "[01:23:45] ..... CRIT: Formattable.")]
    public void Write_Formattable_Mono(LogLevel logLevel, string expected)
    {
        WriteFormattable(logLevel).Should().Be(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243m    : Formattable.~[0m")]
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mtrce: Formattable.~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[01:23:45] ~[34;38;5;23m..... ~[90;38;5;243mdbug: Formattable.~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[39minfo: Formattable.~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[33mwarn: Formattable.~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;41;1mFAIL~[91;49m: Formattable.~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[01:23:45] ~[36;38;5;31m..... ~[97;45;1mCRIT~[95;49m: Formattable.~[0m")]
    public void Write_Formattable_Color(LogLevel logLevel, string expected)
    {
        WriteFormattable(logLevel, color: true).Should().Be(Lines(expected));
    }

    private string Write(
        LogLevel   logLevel  = LogLevel.Information,
        bool       color     = false,
        string?    message   = "Message.",
        Exception? exception = null)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var entry = h.CreateEntry(logLevel, message, exception);

        return h.Write(entry);
    }

    private string WriteFormattable(
        LogLevel   logLevel  = LogLevel.Information,
        bool       color     = false,
        string     content   = "Formattable.",
        Exception? exception = null)
    {
        using var h = new TestHarness(color).AtConstantTime();

        var state = h.CreateFormattable(content, color);
        var entry = h.CreateEntry(logLevel, state, exception);

        return h.AtConstantTime().Write(entry);
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
