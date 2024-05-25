// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using FluentAssertions.Extensions;

namespace Subatomix.Logging.Internal;

[TestFixture]
internal class UtcClockTests
{
    [Test]
    public void Now()
    {
        var now = UtcClock.Instance.Now;

        now.Should().BeCloseTo(DateTime.UtcNow, precision: 5.Seconds());
        now.Kind.Should().Be(DateTimeKind.Utc);
    }
}
