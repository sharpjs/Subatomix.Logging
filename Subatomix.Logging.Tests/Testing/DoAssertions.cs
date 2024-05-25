// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;

namespace Subatomix.Logging.Testing;

using static LogLevel;

internal class DoAssertions
{
    public static DoAssertions Instance { get; } = new();

    public virtual void AssertDoNotStarted(FakeLogger logger)
    {
        logger.Entries.Should().BeEmpty();
    }

    public virtual void AssertDoStarted(FakeLogger logger, string name)
    {
        logger.Entries.Should().HaveCount(1);

        var e0 = logger.Entries[0];

        e0.LogLevel .Should().Be(Information);
        e0.Message  .Should().Be($"{name}: Starting");
        e0.Exception.Should().BeNull();
    }

    public virtual void AssertDoCompleted(FakeLogger logger, string name)
    {
        logger.Entries.Should().HaveCount(2);
 
        var e1 = logger.Entries[1];

        e1.LogLevel .Should().Be(Information);
        e1.Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d\d\ds\]$");
        e1.Exception.Should().BeNull();
    }

    public virtual void AssertDoCompleted(FakeLogger logger, string name, Exception exception)
    {
        logger.Entries.Should().HaveCount(3);
 
        var e1 = logger.Entries[1];
        var e2 = logger.Entries[2];

        e1.LogLevel .Should().Be(Error);
        e1.Message  .Should().BeNullOrEmpty();
        e1.Exception.Should().BeSameAs(exception);
 
        e2.LogLevel .Should().Be(Information);
        e2.Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d\d\ds\] \[EXCEPTION\]$");
        e2.Exception.Should().BeNull();
    }
}
