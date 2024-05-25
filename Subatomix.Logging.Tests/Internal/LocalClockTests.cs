// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using FluentAssertions.Extensions;

namespace Subatomix.Logging.Internal;

[TestFixture]
internal class LocalClockTests
{
    [Test]
    public void Now()
    {
        var now = LocalClock.Instance.Now;

        now.Should().BeCloseTo(DateTime.Now, precision: 5.Seconds());
        now.Kind.Should().Be(DateTimeKind.Local);
    }
}
