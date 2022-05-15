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

    private class TestHarness : TestHarnessBase
    {
        public TestOptionsMonitor<Options> Options { get; }

        public PrettyConsoleFormatter Formatter { get; }

        public TestHarness()
        {
            Options   = new(Mocks);
            Formatter = new PrettyConsoleFormatter(Options);
        }

        protected override void Verify()
        {
            ((IDisposable) Formatter).Dispose();
            base.Verify();
        }
    }
}
