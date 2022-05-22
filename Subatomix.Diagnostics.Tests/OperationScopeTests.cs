/*
    Copyright 2022 Jeffrey Sharp

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

using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Diagnostics.Testing;

namespace Subatomix.Diagnostics;

using static ExceptionTestHelpers;

[TestFixture]
public class OperationScopeTests
{
    [Test]
    public void Construct_NullLogger()
    {
        Invoking(() => new OperationScope(null!, default, "a"))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Test]
    public void Construct_NullName()
    {
        Invoking(() => new OperationScope(NullLogger.Instance, default, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void Construct_EmptyName()
    {
        Invoking(() => new OperationScope(NullLogger.Instance, default, ""))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Logger_Get()
    {
        var logger = new TestLogger();

        using var s = new OperationScope(logger, LogLevel.Debug, "a");

        s.Logger.Should().BeSameAs(logger);
    }

    [Test]
    public void LogLevel_Get()
    {
        var logLevel = Any.NextEnum<LogLevel>();

        using var s = new OperationScope(NullLogger.Instance, logLevel, "a");

        s.LogLevel.Should().Be(logLevel);
    }

    [Test]
    public void Name_Get()
    {
        var name = Any.GetString();

        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, name);

        s.Name.Should().BeSameAs(name);
    }

    [Test]
    public void Exception_Get()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        s.Exception.Should().BeNull();
    }

    [Test]
    public void Exception_Set()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        var exception = Thrown("e");

        s.Exception = exception;
        s.Exception.Should().BeSameAs(exception);
    }

    [Test]
    public void ExceptionLogLevel_Get()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        s.ExceptionLogLevel.Should().Be(LogLevel.Error);
    }

    [Test]
    public void ExceptionLogLevel_Set()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        s.ExceptionLogLevel = LogLevel.Critical;
        s.ExceptionLogLevel.Should().Be(LogLevel.Critical);
    }

    [Test]
    public void LogScope()
    {
        var logger = new TestLogger();
        var name   = Any.GetString();

        using (new OperationScope(logger, LogLevel.Debug, name))
        {
            logger.Scopes.Should().ContainSingle()
                .Which.State.Should().BeSameAs(name);
        }

        logger.Scopes.Should().BeEmpty();
    }

    [Test]
    public async Task Output_Success()
    {
        var logger = new TestLogger();
        var level  = Any.NextEnum<LogLevel>();
        var name   = Any.GetString();

        using (new OperationScope(logger, level, name))
        {
            logger.Entries             .Should().HaveCount(1);
            logger.Entries[0].LogLevel .Should().Be(level);
            logger.Entries[0].Message  .Should().Be($"{name}: Starting");
            logger.Entries[0].Exception.Should().BeNull();

            // Pretend to do some work
            await Task.Delay(10.Milliseconds());
        }

        logger.Entries             .Should().HaveCount(2);
        logger.Entries[1].LogLevel .Should().Be(level);
        logger.Entries[1].Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\]$");
        logger.Entries[1].Message  .Should().NotContain("[0.000s]");
        logger.Entries[1].Exception.Should().BeNull();
    }

    [Test]
    public async Task Output_Exception()
    {
        var logger    = new TestLogger();
        var levelA    = Any.NextEnum<LogLevel>();
        var levelB    = Any.NextEnum<LogLevel>();
        var name      = Any.GetString();
        var exception = Thrown("An error occurred for testing.");

        using (var s = new OperationScope(logger, levelA, name))
        {
            logger.Entries             .Should().HaveCount(1);
            logger.Entries[0].LogLevel .Should().Be(levelA);
            logger.Entries[0].Message  .Should().Be($"{name}: Starting");
            logger.Entries[0].Exception.Should().BeNull();

            // Pretend to do some work
            await Task.Delay(10.Milliseconds());

            s.Exception         = exception;
            s.ExceptionLogLevel = levelB;
        }

        logger.Entries             .Should().HaveCount(3);

        logger.Entries[1].LogLevel .Should().Be(levelB);
        logger.Entries[1].Message  .Should().BeNullOrEmpty();
        logger.Entries[1].Exception.Should().BeSameAs(exception);

        logger.Entries[2].LogLevel .Should().Be(levelA);
        logger.Entries[2].Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\] \[EXCEPTION\]$");
        logger.Entries[2].Message  .Should().NotContain("[0.000s]");
        logger.Entries[2].Exception.Should().BeNull();
    }
}
