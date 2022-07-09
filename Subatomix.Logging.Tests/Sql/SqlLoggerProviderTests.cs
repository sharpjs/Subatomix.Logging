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
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Logging.Internal;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging.Sql;

[TestFixture]
public class SqlLoggerProviderTests
{
    [Test]
    public void Options_Get()
    {
        using var h = new TestHarness();

        h.Provider.Options.Should().BeSameAs(h.Options.CurrentValue);
    }

    [Test]
    public void Options_Change()
    {
        using var h = new TestHarness();

        var options = new SqlLoggerOptions();

        h.Options.CurrentValue = options;
        h.Options.NotifyChanged();

        h.Provider.Options.Should().BeSameAs(options);
    }

    [Test]
    public void Logger_Get()
    {
        using var h = new TestHarness();

        h.Provider.Logger.Should().BeSameAs(NullLogger.Instance);
    }

    [Test]
    public void Logger_Set()
    {
        using var h = new TestHarness();

        var logger = h.Mocks.Create<ILogger>().Object;

        h.Provider.Logger = logger;
        h.Provider.Logger.Should().BeSameAs(logger);
    }

    [Test]
    public void CreateLogger()
    {
        using var h = new TestHarness();

        var name = h.Random.GetString();

        var logger = h.Provider.CreateLogger(name);

        logger.Should().BeOfType<SqlLogger>()
            .Which.AssignTo(out var sqlLogger);

        sqlLogger.Name    .Should().BeSameAs(name);
        sqlLogger.Provider.Should().BeSameAs(h.Provider);
    }

    [Test]
    public void CreateLogger_NullName()
    {
        using var h = new TestHarness();

        h.Provider
            .Invoking(p => p.CreateLogger(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Run_Initial()
    {
        using var h = new TestHarness();

        var wait = h.Options.CurrentValue.AutoflushWait;

        h.Provider.FlushTime.Should().Be(default);
        h.Provider.RetryTime.Should().Be(default);

        h.Provider.ScheduleNextFlush();

        h.Provider.FlushTime.Should().Be(h.BaseDate + wait);
        h.Provider.RetryTime.Should().Be(default);
    }

    [Test]
    public void Tick_Normal()
    {
        var timeout          = Any.Next(1, 300).Seconds();
        var connectionString = Any.GetString();
        var entry            = new LogEntry();

        using var h = new TestHarness(o =>
        {
            o.ConnectionString = connectionString;
            o.BatchTimeout = timeout;
        });

        h.Provider.ScheduleNextFlush();
        h.Provider.Enqueue(entry);

        h.Repository
            .Setup(r => r.TryEnsureConnection(connectionString))
            .Returns(true)
            .Verifiable();

        h.Repository
            .Setup(r => r.Write(h.LogName, It.Is<IEnumerable<LogEntry>>(l => l.SequenceEqual(new[] { entry })), timeout))
            .Verifiable();

        var wait = h.Options.CurrentValue.AutoflushWait;
        h.Clock.Now = h.BaseDate + wait - 5.Milliseconds();

        var result = h.Tick(retryCount: 0);

        result.Elapsed       .Should().BeGreaterThanOrEqualTo(5.Milliseconds());
        result.ShouldContinue.Should().BeTrue();
        result.RetryCount    .Should().Be(0);
    }

    [Test]
    public void Tick_Exiting()
    {
        using var h = new TestHarness();

        h.Provider.ScheduleNextFlush();
        h.Provider.Enqueue(new()); // will not be written // TODO: maybe change this
        h.Provider.Dispose();

        var flushTime = h.Provider.FlushTime;
        var retryTime = h.Provider.RetryTime;
        var anyCount  = Any.Next();

        var result = h.Tick(anyCount);

        result.Elapsed       .Should().BeCloseTo(TimeSpan.Zero, precision: 1.Milliseconds());
        result.ShouldContinue.Should().BeFalse();
        result.RetryCount    .Should().Be(anyCount);

        h.Provider.FlushTime.Should().Be(flushTime);
        h.Provider.RetryTime.Should().Be(retryTime);
    }

    [Test]
    public void Tick_Empty()
    {
        using var h = new TestHarness(o =>
        {
            o.AutoflushWait = 50.Milliseconds();
        });

        var retries = 0;

        h.Clock.Now = DateTime.UtcNow - 30.Minutes();

        h.Provider.Tick(ref retries).Should().BeTrue(); retries.Should().Be(0);

        h.Provider.Tick(ref retries).Should().BeTrue();
        retries.Should().Be(0);
    }

    [Test]
    public void Dispose()
    {
        using var h = new TestHarness();

        // First Dispose() should dispose managed objects
        h.Provider.Dispose();
        h.Mocks.Verify();

        // Second Dispose(), via using, should have no effect
        h.Repository.Reset();
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var h = new TestHarness();

        h.Provider.SimulateUnmanagedDisposal();
    }

    private class TestHarness : TestHarnessBase
    {
        public SqlLoggerProvider Provider { get; }

        public Mock<ISqlLogRepository> Repository { get; }

        public TestClock Clock { get; }
        public DateTime BaseDate { get; }

        public TestOptionsMonitor<SqlLoggerOptions> Options { get; }

        public string LogName { get; } = Any.GetString();

        public TestHarness(Action<SqlLoggerOptions>? configure = null)
        {
            BaseDate = DateTime.UtcNow - 10.Minutes();

            Options = new(Mocks);
            var o = Options.CurrentValue;
            o.LogName = LogName;
            configure?.Invoke(Options.CurrentValue);

            Repository = Mocks.Create<ISqlLogRepository>();
            Repository.Setup(r => r.Dispose()).Verifiable();

            Clock = new TestClock { Now = BaseDate };

            Provider = new(Options, Repository.Object, Clock, startThread: false);
        }

        public void Schedule(TimeSpan flushTime, TimeSpan retryTime)
        {
            var flushDate = BaseDate + flushTime;
            var retryDate = BaseDate + retryTime;
        }

        public TickResult Tick(int retryCount)
        {
            var stopwatch = Stopwatch.StartNew();
            var result    = Provider.Tick(ref retryCount);
            stopwatch.Stop();

            return new(result, retryCount, stopwatch.Elapsed);
        }

        protected override void Verify()
        {
            Provider.Dispose();
            base.Verify();
        }
    }

    private record TickResult(
        bool     ShouldContinue,
        int      RetryCount,
        TimeSpan Elapsed
    );
}
