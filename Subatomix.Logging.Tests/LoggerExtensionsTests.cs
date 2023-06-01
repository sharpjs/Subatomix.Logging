/*
    Copyright 2023 Jeffrey Sharp

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
