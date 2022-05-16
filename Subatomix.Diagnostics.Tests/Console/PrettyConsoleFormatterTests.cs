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
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Subatomix.Diagnostics.Internal;
using Subatomix.Diagnostics.Testing;
using Subatomix.Testing;

namespace Subatomix.Diagnostics.Console;

using Options      = PrettyConsoleFormatterOptions;
using Subformatter = Func<string?, Exception?, string>;

[TestFixture]
[SetCulture("kl-GL")] // Kalaallisut (Greenland), to verify nonreliance on en-US
public class PrettyConsoleFormatterTests
{
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
        var optionsB = new Options();

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
    [TestCase(LoggerColorBehavior.Disabled, false)]
    [TestCase(LoggerColorBehavior.Enabled,  true )]
    public void IsColorEnabled_Explicit(LoggerColorBehavior behavior, bool expected)
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
    [TestCase(LogLevel.None,        "[??:??:??] .....       : Message.")]
    [TestCase(LogLevel.Trace,       "[??:??:??] ..... trace : Message.")]
    [TestCase(LogLevel.Debug,       "[??:??:??] ..... debug : Message.")]
    [TestCase(LogLevel.Information, "[??:??:??] ..... info  : Message.")]
    [TestCase(LogLevel.Warning,     "[??:??:??] ..... warn  : Message.")]
    [TestCase(LogLevel.Error,       "[??:??:??] .....  ERR  : Message.")]
    [TestCase(LogLevel.Critical,    "[??:??:??] .....  RIP  : Message.")]
    public void Write_Message_Mono(LogLevel logLevel, string expected)
    {
        Write(logLevel).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "[??:??:??] #????       : Message.")]
    [TestCase(LogLevel.Trace,       "[??:??:??] #???? trace : Message.")]
    [TestCase(LogLevel.Debug,       "[??:??:??] #???? debug : Message.")]
    [TestCase(LogLevel.Information, "[??:??:??] #???? info  : Message.")]
    [TestCase(LogLevel.Warning,     "[??:??:??] #???? warn  : Message.")]
    [TestCase(LogLevel.Error,       "[??:??:??] #????  ERR  : Message.")]
    [TestCase(LogLevel.Critical,    "[??:??:??] #????  RIP  : Message.")]
    public void Write_Message_Mono_InActivity(LogLevel logLevel, string expected)
    {
        using var _ = new Activity("Test").SetIdFormat(ActivityIdFormat.W3C).Start();

        Write(logLevel).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "~[90;38;5;239m[??:??:??] ~[34;38;5;23m..... ~[90;38;5;243m      : Message.~[0m")]
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m..... ~[90;38;5;243mtrace : Message.~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m..... ~[90;38;5;243mdebug : Message.~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[39minfo  : Message.~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[33mwarn  : Message.~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[97;41;1m ERR ~[91;49m : Message.~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[97;45;1m RIP ~[95;49m : Message.~[0m")]
    public void Write_Message_Color(LogLevel logLevel, string expected)
    {
        Write(logLevel, colors: true).Should().Match(Lines(expected));
    }

    [Test]
    [TestCase(LogLevel.None,        "~[90;38;5;239m[??:??:??] ~[34;38;5;23m#???? ~[90;38;5;243m      : Message.~[0m")]
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m#???? ~[90;38;5;243mtrace : Message.~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m#???? ~[90;38;5;243mdebug : Message.~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[??:??:??] ~[36;38;5;31m#???? ~[39minfo  : Message.~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[??:??:??] ~[36;38;5;31m#???? ~[33mwarn  : Message.~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[??:??:??] ~[36;38;5;31m#???? ~[97;41;1m ERR ~[91;49m : Message.~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[??:??:??] ~[36;38;5;31m#???? ~[97;45;1m RIP ~[95;49m : Message.~[0m")]
    public void Write_Message_Color_InActivity(LogLevel logLevel, string expected)
    {
        using var _ = new Activity("Test").SetIdFormat(ActivityIdFormat.W3C).Start();

        Write(logLevel, colors: true).Should().Match(Lines(expected));
    }

    [Test]
    public void Write_Nothing()
    {
        Write(message: null).Should().BeEmpty();
    }

    [Test]
    public void Write_Exception()
    {
        var e = CreateThrownException();

        Write(message: null, exception: e).Should().Match(Lines(
            "[??:??:??] ..... info  : System.ApplicationException: A test error was thrown.",
            "   at Subatomix.Diagnostics.PrettyConsoleFormatterTests.CreateThrownException()*"
        ));
    }

    [Test]
    public void Write_MessageAndException()
    {
        var e = CreateThrownException();

        Write(exception: e).Should().Match(Lines(
            "[??:??:??] ..... info  : Message. System.ApplicationException: A test error was thrown.",
            "   at Subatomix.Diagnostics.PrettyConsoleFormatterTests.CreateThrownException()*"
        ));
    }

    private string Write(
        LogLevel   logLevel  = LogLevel.Information,
        bool       colors    = false,
        string?    message   = "Message.",
        Exception? exception = null)
    {
        var behavior = colors
            ? LoggerColorBehavior.Enabled
            : LoggerColorBehavior.Disabled;

        using var h = new TestHarness(o => o.ColorBehavior = behavior);

        h.Formatter.Clock = TestClock;

        // Cover both ways in which entry.Formatter can yield a null message
        var subformatter = (message, exception) is (null, null)
            ? null                                      // case A: .Formatter is null
            : h.CreateSubformatter(message, exception); // case B: .Formatter might return null

        var entry = new LogEntry<string?>(
            logLevel, "category", eventId: 42, message, exception, subformatter!
        );

        using var writer = new StringWriter();

        h.Formatter.Write(entry, h.Scopes, writer);

        return writer.ToString().Replace('\x1B', '~');
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

    private static Exception CreateThrownException()
    {
        try { throw new ApplicationException("A test error was thrown."); }
        catch (Exception e) { return e; }
    }

    private readonly TestClock TestClock = new()
    {
        Now = DateTime.UtcNow.Date + new TimeSpan(1, 23, 45)
    };

    private class TestHarness : TestHarnessBase
    {
        public TestOptionsMonitor<Options> Options { get; }

        public IExternalScopeProvider Scopes { get; }

        public PrettyConsoleFormatter Formatter { get; }

        public TestHarness(Action<Options>? configure = null)
        {
            Options = new TestOptionsMonitor<Options>(Mocks);
            configure?.Invoke(Options.CurrentValue);

            Scopes = Mocks.Create<IExternalScopeProvider>().Object;

            Formatter = new PrettyConsoleFormatter(Options);
        }

        public Subformatter CreateSubformatter(string? message, Exception? exception)
        {
            var mock = Mocks.Create<Subformatter>();

            mock.Setup(f => f(message, exception))
                .Returns(message!)
                .Verifiable();

            return mock.Object;
        }

        protected override void Verify()
        {
            ((IDisposable) Formatter).Dispose();
            base.Verify();
        }
    }
}
