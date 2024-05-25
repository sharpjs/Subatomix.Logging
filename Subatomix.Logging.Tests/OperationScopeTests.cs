// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Logging.Console;
using Subatomix.Logging.Fake;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging;

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
        var logger = new FakeLogger();

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
    public void IsCompleted_Get_NotCompleted()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        s.IsCompleted.Should().BeFalse();
    }

    [Test]
    public void IsCompleted_Get_Completed()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        ((IDisposable) s).Dispose();

        s.IsCompleted.Should().BeTrue();
    }

    [Test]
    public async Task Duration_Get_NotCompleted()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        var previousDuration = s.Duration;
        await Task.Delay(10.Milliseconds());

        s.Duration.Should().BeGreaterThan(previousDuration);
    }

    [Test]
    public async Task Duration_Get_Completed()
    {
        using var s = new OperationScope(NullLogger.Instance, LogLevel.Debug, "a");

        ((IDisposable) s).Dispose();

        var previousDuration = s.Duration;
        await Task.Delay(10.Milliseconds());

        s.Duration.Should().Be(previousDuration);
    }

    [Test]
    public void LogScope()
    {
        var logger = new FakeLogger();
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
        var logger = new FakeLogger();
        var level  = Any.NextEnum<LogLevel>();
        var name   = Any.GetString();

        using var s = new OperationScope(logger, level, name);

        logger.Entries             .Should().HaveCount(1);
        logger.Entries[0].LogLevel .Should().Be(level);
        logger.Entries[0].Message  .Should().Be($"{name}: Starting");
        logger.Entries[0].Exception.Should().BeNull();

        FormatForConsole(s, false).Should().Be($"{name}: Starting");
        FormatForConsole(s, true ).Should().Be($"~[1m{name}: ~[96;22mStarting");

        // Pretend to do some work
        await Task.Delay(10.Milliseconds());

        ((IDisposable) s).Dispose();

        logger.Entries             .Should().HaveCount(2);
        logger.Entries[1].LogLevel .Should().Be(level);
        logger.Entries[1].Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\]$");
        logger.Entries[1].Message  .Should().NotContain("[0.000s]");
        logger.Entries[1].Exception.Should().BeNull();

        FormatForConsole(s, false).Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\]$");
        FormatForConsole(s, true ).Should().MatchRegex(
            $@"^~\[1m{name}: ~\[96;22mCompleted~\[37;38;5;248m \[\d+\.\d{{3}}s\]$"
        );
    }

    [Test]
    public async Task Output_Exception()
    {
        var logger    = new FakeLogger();
        var levelA    = Any.NextEnum<LogLevel>();
        var levelB    = Any.NextEnum<LogLevel>();
        var name      = Any.GetString();
        var exception = Thrown("An error occurred for testing.");

        using var s = new OperationScope(logger, levelA, name);

        logger.Entries             .Should().HaveCount(1);
        logger.Entries[0].LogLevel .Should().Be(levelA);
        logger.Entries[0].Message  .Should().Be($"{name}: Starting");
        logger.Entries[0].Exception.Should().BeNull();

        FormatForConsole(s, false).Should().Be($"{name}: Starting");
        FormatForConsole(s, true ).Should().Be($"~[1m{name}: ~[96;22mStarting");
        // Pretend to do some work
        await Task.Delay(10.Milliseconds());

        s.Exception         = exception;
        s.ExceptionLogLevel = levelB;

        ((IDisposable) s).Dispose();

        logger.Entries             .Should().HaveCount(3);

        logger.Entries[1].LogLevel .Should().Be(levelB);
        logger.Entries[1].Message  .Should().BeNullOrEmpty();
        logger.Entries[1].Exception.Should().BeSameAs(exception);

        logger.Entries[2].LogLevel .Should().Be(levelA);
        logger.Entries[2].Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\] \[EXCEPTION\]$");
        logger.Entries[2].Message  .Should().NotContain("[0.000s]");
        logger.Entries[2].Exception.Should().BeNull();

        FormatForConsole(s, false).Should().MatchRegex($@"^{name}: Completed \[\d+\.\d{{3}}s\] \[EXCEPTION\]$");
        FormatForConsole(s, true ).Should().MatchRegex(
            $@"^~\[1m{name}: ~\[96;22mCompleted~\[37;38;5;248m \[\d+\.\d{{3}}s\]~\[1;33m \[EXCEPTION\]$"
        );
    }

    [Test]
    public void Stop_Multiple()
    {
        var logger = new FakeLogger();

        using (var s = new OperationScope(logger))
        {
            logger.Entries.Should().HaveCount(1);
            logger.Scopes .Should().HaveCount(1);

            ((IDisposable) s).Dispose();

            logger.Entries.Should().HaveCount(2);
            logger.Scopes .Should().HaveCount(0);

            // Second implicit disposal via using is harmless
        }

        logger.Entries.Should().HaveCount(2);
        logger.Scopes .Should().HaveCount(0);
    }

    private static string FormatForConsole(IConsoleFormattable formattable, bool color = true)
    {
        const string Reset
            = Ansi.Begin + Ansi.Reset + Ansi.End;

        using var writer = new StringWriter();

        formattable.Write(writer, new(color ? Reset : null));

        return writer.ToString().Replace('\x1B', '~');
    }
}
