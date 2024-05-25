// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using FluentAssertions.Extensions;
using Subatomix.Logging.Internal;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging.Sql;

[TestFixture]
public class SqlLoggerTests
{
    [Test]
    public void Construct_NullProvider()
    {
        Invoking(() => new SqlLogger(null!, "any"))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("provider");
    }

    [Test]
    public void Construct_NullName()
    {
        var provider = Mock.Of<ISqlLoggerProvider>();

        Invoking(() => new SqlLogger(provider, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void IsEnabled_Get()
    {
        using var h = new TestHarness();

        h.Logger.IsEnabled(Any.LogLevelExceptNone()).Should().BeTrue();
    }

    [Test]
    public void BeginScope()
    {
        using var h = new TestHarness();

        h.Logger.BeginScope(new object()).Should().BeSameAs(NullScope.Instance);
    }

    [Test]
    [TestCase(null,  null)]
    [TestCase("",    null)]
    [TestCase("NEF", null)]
    public void Log_Empty(string? formatted, string? error)
    {
        using var h = new TestHarness();

        var logLevel  = Any.LogLevelExceptNone();
        var eventId   = (EventId) Any.Next();
        var state     = new State();

        string Format(State s, Exception? e)
        {
            s.Should().BeSameAs(state);
            e.Should().BeNull();
            return formatted!;
        }

        h.Logger.Log(logLevel, eventId, state, null, formatted is "NEF" ? null! : Format);

        h.Entries.Should().BeEmpty();
    }

    [Test]
    [TestCase("Message.", null, "Message.")]
    [TestCase(null,       "e",  "System.Exception: e")]
    [TestCase("",         "e",  "System.Exception: e")]
    [TestCase("NEF",      "e",  "System.Exception: e")]
    [TestCase("Message.", "e",  "Message. System.Exception: e")]
    public void Log_NotEmpty(string? formatted, string? error, string expected)
    {
        using var h = new TestHarness();

        var logLevel  = Any.LogLevelExceptNone();
        var eventId   = (EventId) Any.Next();
        var state     = new State();
        var exception = error is null ? null : new Exception(error);

        string Format(State s, Exception? e)
        {
            s.Should().BeSameAs(state);
            e.Should().BeSameAs(exception);
            return formatted!;
        }

        h.Logger.Log(logLevel, eventId, state, exception, formatted is "NEF" ? null! : Format);

        h.Entries.Should().HaveCount(1);

        var entry = h.Entries[0];
        entry.Level   .Should().Be(logLevel);
        entry.EventId .Should().Be(eventId.Id);
        entry.Message .Should().Be(expected);
        entry.Category.Should().Be(h.Logger.Name);
        entry.Date    .Should().BeCloseTo(DateTime.UtcNow, precision: 50.Milliseconds());
        entry.TraceId .Should().BeNull();
            
    }

    [Test]
    [TestCase("Message.", null, "Message.")]
    [TestCase(null,       "e",  "System.Exception: e")]
    [TestCase("",         "e",  "System.Exception: e")]
    [TestCase("NEF",      "e",  "System.Exception: e")]
    [TestCase("Message.", "e",  "Message. System.Exception: e")]
    [NonParallelizable] // becuase it depends on static state
    public void Log_NotEmpty_InActivity_W3C(string? formatted, string? error, string expected)
    {
        using var _ = new ActivityTestScope(ActivityIdFormat.W3C);
        using var a = new Activity("Test").Start();
        using var h = new TestHarness();

        var logLevel  = Any.LogLevelExceptNone();
        var eventId   = (EventId) Any.Next();
        var state     = new State();
        var exception = error is null ? null : new Exception(error);

        string Format(State s, Exception? e)
        {
            s.Should().BeSameAs(state);
            e.Should().BeSameAs(exception);
            return formatted!;
        }

        h.Logger.Log(logLevel, eventId, state, exception, formatted is "NEF" ? null! : Format);

        h.Entries.Should().HaveCount(1);

        var entry = h.Entries[0];
        entry.Level   .Should().Be(logLevel);
        entry.EventId .Should().Be(eventId.Id);
        entry.Message .Should().Be(expected);
        entry.Category.Should().Be(h.Logger.Name);
        entry.Date    .Should().BeCloseTo(DateTime.UtcNow, precision: 50.Milliseconds());
        entry.TraceId .Should().Be(a.TraceId.ToString());
    }

    [Test]
    [TestCase("Message.", null, "Message.")]
    [TestCase(null,       "e",  "System.Exception: e")]
    [TestCase("",         "e",  "System.Exception: e")]
    [TestCase("NEF",      "e",  "System.Exception: e")]
    [TestCase("Message.", "e",  "Message. System.Exception: e")]
    [NonParallelizable] // becuase it depends on static state
    public void Log_NotEmpty_InActivity_Hierarchical(string? formatted, string? error, string expected)
    {
        using var _ = new ActivityTestScope(ActivityIdFormat.Hierarchical);
        using var a = new Activity("Test").Start();
        using var h = new TestHarness();

        var logLevel  = Any.LogLevelExceptNone();
        var eventId   = (EventId) Any.Next();
        var state     = new State();
        var exception = error is null ? null : new Exception(error);

        string Format(State s, Exception? e)
        {
            s.Should().BeSameAs(state);
            e.Should().BeSameAs(exception);
            return formatted!;
        }

        h.Logger.Log(logLevel, eventId, state, exception, formatted is "NEF" ? null! : Format);

        h.Entries.Should().HaveCount(1);

        var entry = h.Entries[0];
        entry.Level   .Should().Be(logLevel);
        entry.EventId .Should().Be(eventId.Id);
        entry.Message .Should().Be(expected);
        entry.Category.Should().Be(h.Logger.Name);
        entry.Date    .Should().BeCloseTo(DateTime.UtcNow, precision: 50.Milliseconds());
        entry.TraceId .Should().Be(a.RootId);
    }

    private class State { }

    private class TestHarness : TestHarnessBase, ISqlLoggerProvider
    {
        public SqlLogger Logger { get; }

        public List<LogEntry> Entries { get; }

        public TestHarness()
        {
            Entries = new();
            Logger  = new(this, Random.GetString());
        }

        public void Enqueue(LogEntry entry)
            => Entries.Add(entry);
    }
}
