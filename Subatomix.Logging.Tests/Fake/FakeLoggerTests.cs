// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Fake;

[TestFixture]
public class FakeLoggerTests
{
    [Test]
    public void MinimumLevel()
    {
        var logger = new FakeLogger();

        logger.MinimumLevel.Should().Be(LogLevel.Trace);

        logger.MinimumLevel = LogLevel.Warning;

        logger.MinimumLevel.Should().Be(LogLevel.Warning);
    }

    [Test]
    public void IsEnabled()
    {
        var logger = new FakeLogger() {  MinimumLevel = LogLevel.Information };

        logger.IsEnabled(LogLevel.Warning)    .Should().BeTrue();
        logger.IsEnabled(LogLevel.Information).Should().BeTrue();
        logger.IsEnabled(LogLevel.Debug)      .Should().BeFalse();
    }

    [Test]
    public void BeginScope()
    {
        var logger = new FakeLogger();

        using var scope = logger.BeginScope(this);

        scope.Should().BeOfType<FakeLogger.Scope<FakeLoggerTests>>()
            .Which.State.Should().BeSameAs(this);

        logger.Scopes.Should().HaveCount(1);
        logger.Scopes.First().Should().BeSameAs(scope);
    }

    [Test]
    public void BeginScope_DisposeMultiple()
    {
        var logger = new FakeLogger();

        var scope = logger.BeginScope(this)!;

        scope.Should().BeOfType<FakeLogger.Scope<FakeLoggerTests>>();

        scope.Dispose();

        scope.Invoking(s => s.Dispose())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Attempted to dispose a scope other than the innermost scope.");
    }

    [Test]
    public void BeginScope_DisposeOutOfOrder()
    {
        var logger = new FakeLogger();

        using var scope0 = logger.BeginScope(this)!;
        using var scope1 = logger.BeginScope(this)!;

        scope0.Should().BeOfType<FakeLogger.Scope<FakeLoggerTests>>();
        scope1.Should().BeOfType<FakeLogger.Scope<FakeLoggerTests>>();

        scope0.Invoking(s => s.Dispose())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Attempted to dispose a scope other than the innermost scope.");
    }
}
