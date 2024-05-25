// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;

namespace Subatomix.Logging.Testing;

internal class ActivityDoAssertions : DoAssertions
{
    public static new ActivityDoAssertions Instance { get; } = new();

    public override void AssertDoNotStarted(FakeLogger logger)
    {
        base.AssertDoNotStarted(logger);

        Activity.Current.Should().BeNull();
    }

    public override void AssertDoStarted(FakeLogger logger, string name)
    {
        base.AssertDoStarted(logger, name);

        Activity.Current               .Should().NotBeNull();
        Activity.Current!.OperationName.Should().Be("a");
    }

    public override void AssertDoCompleted(FakeLogger logger, string name)
    {
        base.AssertDoCompleted(logger, name);

        Activity.Current.Should().BeNull();
    }

    public override void AssertDoCompleted(FakeLogger logger, string name, Exception exception)
    {
        base.AssertDoCompleted(logger, name, exception);

        Activity.Current.Should().BeNull();
    }
}
