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
