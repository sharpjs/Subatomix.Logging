using FluentAssertions.Extensions;

namespace Subatomix.Diagnostics;

[TestFixture]
internal class UtcClockTests
{
    [Test]
    public void Now()
    {
        var now = UtcClock.Instance.Now;

        now     .Should().BeCloseTo(DateTime.UtcNow, precision: 5.Seconds());
        now.Kind.Should().Be(DateTimeKind.Utc);
    }
}
