// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging;

using static ExceptionTestHelpers;
using static LogLevel;

[TestFixture]
public partial class LoggerExtensionsTests
{
    [Test]
    public void LogTrace_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogTrace(exception);

        logger.Entries.Should().Equal((Trace, 0, "", exception));
    }

    [Test]
    public void LogDebug_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogDebug(exception);

        logger.Entries.Should().Equal((Debug, 0, "", exception));
    }

    [Test]
    public void LogInformation_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogInformation(exception);

        logger.Entries.Should().Equal((Information, 0, "", exception));
    }

    [Test]
    public void LogWarning_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogWarning(exception);

        logger.Entries.Should().Equal((Warning, 0, "", exception));
    }

    [Test]
    public void LogError_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogError(exception);

        logger.Entries.Should().Equal((Error, 0, "", exception));
    }

    [Test]
    public void LogCritical_Exception()
    {
        var logger    = new FakeLogger();
        var exception = Thrown("a");

        logger.LogCritical(exception);

        logger.Entries.Should().Equal((Critical, 0, "", exception));
    }

    [Test]
    public void Operation_Name()
    {
        var logger = new FakeLogger();

        // Relying on [CallerMemberName] for name
        using var scope = logger.Operation().Begin();

        scope         .Should().NotBeNull();
        scope.Name    .Should().Be(nameof(Operation_Name));
        scope.Logger  .Should().BeSameAs(logger);
        scope.LogLevel.Should().Be(Information);
    }

    [Test]
    public void Operation_LevelAndName()
    {
        var logger = new FakeLogger();

        // Relying on [CallerMemberName] for name
        using var scope = logger.Operation(Debug).Begin();

        scope         .Should().NotBeNull();
        scope.Name    .Should().Be(nameof(Operation_LevelAndName));
        scope.Logger  .Should().BeSameAs(logger);
        scope.LogLevel.Should().Be(Debug);
    }

    [Test]
    public void Activity_Name()
    {
        var logger = new FakeLogger();

        // Relying on [CallerMemberName] for name
        using var scope = logger.Activity().Begin();

        scope         .Should().NotBeNull();
        scope.Name    .Should().Be(nameof(Activity_Name));
        scope.Logger  .Should().BeSameAs(logger);
        scope.LogLevel.Should().Be(Information);
    }

    [Test]
    public void Activity_LevelAndName()
    {
        var logger = new FakeLogger();

        // Relying on [CallerMemberName] for name
        using var scope = logger.Activity(Debug).Begin();

        scope         .Should().NotBeNull();
        scope.Name    .Should().Be(nameof(Activity_LevelAndName));
        scope.Logger  .Should().BeSameAs(logger);
        scope.LogLevel.Should().Be(Debug);
    }
}
