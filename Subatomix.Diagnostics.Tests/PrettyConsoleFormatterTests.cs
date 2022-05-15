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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Subatomix.Testing;

namespace Subatomix.Diagnostics;

using Options = PrettyConsoleFormatterOptions;

[TestFixture]
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
    public void IsColorEnabled_Enabled()
    {
        using var h = new TestHarness(options =>
        {
            options.ColorBehavior = LoggerColorBehavior.Enabled;
        });

        h.Formatter.IsColorEnabled.Should().BeTrue();

        h.Formatter.IsConsoleRedirected = false;
        h.Options.NotifyChanged();

        h.Formatter.IsColorEnabled.Should().BeTrue();

        h.Formatter.IsConsoleRedirected = true;
        h.Options.NotifyChanged();

        h.Formatter.IsColorEnabled.Should().BeTrue();
    }

    [Test]
    public void IsColorEnabled_Disabled()
    {
        using var h = new TestHarness(options =>
        {
            options.ColorBehavior = LoggerColorBehavior.Disabled;
        });

        h.Formatter.IsColorEnabled.Should().BeFalse();

        h.Formatter.IsConsoleRedirected = false;
        h.Options.NotifyChanged();

        h.Formatter.IsColorEnabled.Should().BeFalse();

        h.Formatter.IsConsoleRedirected = true;
        h.Options.NotifyChanged();

        h.Formatter.IsColorEnabled.Should().BeFalse();
    }

    [Test]
    [SetCulture("kl-GL")] // Kalaallisut (Greenland)
    [TestCase(LogLevel.Trace,       "[??:??:??] ..... trace : message")]
    [TestCase(LogLevel.Debug,       "[??:??:??] ..... debug : message")]
    [TestCase(LogLevel.Information, "[??:??:??] ..... info  : message")]
    [TestCase(LogLevel.Warning,     "[??:??:??] ..... warn  : message")]
    [TestCase(LogLevel.Error,       "[??:??:??] .....  ERR  : message")]
    [TestCase(LogLevel.Critical,    "[??:??:??] .....  RIP  : message")]
    public void Write_Mono(LogLevel logLevel, string expected)
    {
        using var h = new TestHarness(options =>
        {
            options.ColorBehavior = LoggerColorBehavior.Disabled;
        });

        var entry = new LogEntry<Void>(
            logLevel, "category", eventId: 42, state: default, exception: null,
            (state, exception) => "message"
        );

        using var writer = new StringWriter();

        h.Formatter.Write(entry, h.CreateScopeProvider(), writer);

        writer.ToString().Should().Match(expected + Environment.NewLine);
    }

    [Test]
    [SetCulture("kl-GL")] // Kalaallisut (Greenland)
    [TestCase(LogLevel.Trace,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m..... ~[90;38;5;243mtrace : message~[0m")]
    [TestCase(LogLevel.Debug,       "~[90;38;5;239m[??:??:??] ~[34;38;5;23m..... ~[90;38;5;243mdebug : message~[0m")]
    [TestCase(LogLevel.Information, "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[39minfo  : message~[0m")]
    [TestCase(LogLevel.Warning,     "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[33mwarn  : message~[0m")]
    [TestCase(LogLevel.Error,       "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[97;41;1m ERR ~[91;49m : message~[0m")]
    [TestCase(LogLevel.Critical,    "~[37;38;5;242m[??:??:??] ~[36;38;5;31m..... ~[97;45;1m RIP ~[95;49m : message~[0m")]
    public void Write_Color(LogLevel logLevel, string expected)
    {
        using var h = new TestHarness(options =>
        {
            options.ColorBehavior = LoggerColorBehavior.Enabled;
        });

        var entry = new LogEntry<Void>(
            logLevel, "category", eventId: 42, state: default, exception: null,
            (state, exception) => "message"
        );

        using var writer = new StringWriter();

        h.Formatter.Write(entry, h.CreateScopeProvider(), writer);

        writer.ToString().Replace('\x1B', '~').Should().Match(expected + Environment.NewLine);
    }

    private class TestHarness : TestHarnessBase
    {
        public TestOptionsMonitor<Options> Options { get; }

        public PrettyConsoleFormatter Formatter { get; }

        public TestHarness(Action<Options>? configure = null)
        {
            Options = new TestOptionsMonitor<Options>(Mocks);
            configure?.Invoke(Options.CurrentValue);

            Formatter = new PrettyConsoleFormatter(Options);
        }

        protected override void Verify()
        {
            ((IDisposable) Formatter).Dispose();
            base.Verify();
        }

        public IExternalScopeProvider CreateScopeProvider()
        {
            return Mocks.Create<IExternalScopeProvider>().Object;
        }
    }
}
