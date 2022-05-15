using FluentAssertions.Extensions;

namespace Subatomix.Diagnostics;

[TestFixture]
internal class LocalClockTests
{
    [Test]
    public void Now()
    {
        var now = LocalClock.Instance.Now;

        now     .Should().BeCloseTo(DateTime.Now, precision: 5.Seconds());
        now.Kind.Should().Be(DateTimeKind.Local);
    }
}
