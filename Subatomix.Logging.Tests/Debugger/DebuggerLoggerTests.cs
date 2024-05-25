// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text.RegularExpressions;
using Subatomix.Logging.Internal;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging.Debugger;

using static ExceptionTestHelpers;
using static LogLevel;

[TestFixture]
public class DebuggerLoggerTests
{
    [Test]
    public void Construct_NullProvider()
    {
        var name = Any.GetString();

        Invoking(() => new DebuggerLogger(null!, name))
            .Should().Throw<ArgumentNullException>().WithParameterName("provider");
    }

    [Test]
    public void Construct_NullName()
    {
        using var provider = new DebuggerLoggerProvider();

        Invoking(() => new DebuggerLogger(provider, null!))
            .Should().Throw<ArgumentNullException>().WithParameterName("name");
    }

    [Test]
    public void Provider_Get()
    {
        using var provider = new DebuggerLoggerProvider();

        var name = Any.GetString();

        new DebuggerLogger(provider, name)
            .Provider.Should().BeSameAs(provider);
    }

    [Test]
    public void Name_Get()
    {
        using var provider = new DebuggerLoggerProvider();

        var name = Any.GetString();

        new DebuggerLogger(provider, name)
            .Name.Should().BeSameAs(name);
    }

    [Test]
    public void Debugger_Get()
    {
        using var provider = new DebuggerLoggerProvider();

        var name = Any.GetString();

        new DebuggerLogger(provider, name)
            .Debugger.Should().BeSameAs(Debugger.Instance);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void BeginScope_NoScopeProvider(bool attached)
    {
        using var h = new TestHarness(attached);

        var name = Any.GetString();

        h.Logger.BeginScope(name).Should().BeSameAs(NullScope.Instance);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void BeginScope_ScopeProviderReturnsNull(bool attached)
    {
        using var h = new TestHarness(attached);

        var name = Any.GetString();

        h.Provider.SetScopeProvider(
            Mock.Of<IExternalScopeProvider>(
                p => p.Push(name) == null!,
                MockBehavior.Strict
            )
        );

        h.Logger.BeginScope(name).Should().BeSameAs(NullScope.Instance);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void BeginScope_Normal(bool attached)
    {
        using var h = new TestHarness(attached);

        var name  = Any.GetString();
        var scope = Mock.Of<IDisposable>();

        h.Provider.SetScopeProvider(
            Mock.Of<IExternalScopeProvider>(
                p => p.Push(name) == scope,
                MockBehavior.Strict
            )
        );

        h.Logger.BeginScope(name).Should().BeSameAs(scope);
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void IsEnabled_LogLevelNone(bool attached)
    {
        using var h = new TestHarness(attached);

        h.Logger.IsEnabled(None).Should().BeFalse();
    }

    [Test]
    public void IsEnabled_DebuggerNotAttached()
    {
        using var h = new TestHarness(attached: false);

        var logLevel = h.Random.LogLevelExceptNone();

        h.Logger.IsEnabled(logLevel).Should().BeFalse();
    }

    [Test]
    public void IsEnabled_DebuggerAttached()
    {
        using var h = new TestHarness(attached: true);

        var logLevel = h.Random.LogLevelExceptNone();

        h.Logger.IsEnabled(logLevel).Should().BeTrue();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true )]
    public void Log_LevelNone(bool attached)
    {
        using var h = new TestHarness(attached);

        var eventId   = h.Random.Next();
        var state     = h.Random.GetString();
        var exception = Thrown();

        static string Format(string state, Exception? exception)
            => throw new Exception("Formatter should not be invoked.");

        h.Logger.Log(None, eventId, state, exception, Format);

        h.Debugger.Entries.Should().BeEmpty();
    }

    [Test]
    public void Log_DebuggerNotAttached()
    {
        using var h = new TestHarness(attached: false);

        var logLevel  = h.Random.LogLevelExceptNone();
        var eventId   = h.Random.Next();
        var state     = h.Random.GetString();
        var exception = Thrown();

        static string Format(string state, Exception? exception)
            => throw new Exception("Formatter should not be invoked.");

        h.Logger.Log(logLevel, eventId, state, exception, Format);

        h.Debugger.Entries.Should().BeEmpty();
    }

    [Test]
    // Message
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "message", false, "• trac: message")]
    [TestCase(Debug,       "message", false, "• dbug: message")]
    [TestCase(Information, "message", false, "• info: message")]
    [TestCase(Warning,     "message", false, "• warn: message")]
    [TestCase(Error,       "message", false, "• FAIL: message")]
    [TestCase(Critical,    "message", false, "• CRIT: message")]
    [TestCase(-42,         "message", false, "• ????: message")]
    // Empty message
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "",        false  )]
    [TestCase(Debug,       "",        false  )]
    [TestCase(Information, "",        false  )]
    [TestCase(Warning,     "",        false  )]
    [TestCase(Error,       "",        false  )]
    [TestCase(Critical,    "",        false  )]
    [TestCase(-42,         "",        false  )]
    // Null message
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       null,      false  )]
    [TestCase(Debug,       null,      false  )]
    [TestCase(Information, null,      false  )]
    [TestCase(Warning,     null,      false  )]
    [TestCase(Error,       null,      false  )]
    [TestCase(Critical,    null,      false  )]
    [TestCase(-42,         null,      false  )]
    // Null entry formatter
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "NEF",     false  )]
    [TestCase(Debug,       "NEF",     false  )]
    [TestCase(Information, "NEF",     false  )]
    [TestCase(Warning,     "NEF",     false  )]
    [TestCase(Error,       "NEF",     false  )]
    [TestCase(Critical,    "NEF",     false  )]
    [TestCase(-42,         "NEF",     false  )]
    // Message and exception
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "message", true,  "• trac: message", "• trac: exception")]
    [TestCase(Debug,       "message", true,  "• dbug: message", "• dbug: exception")]
    [TestCase(Information, "message", true,  "• info: message", "• info: exception")]
    [TestCase(Warning,     "message", true,  "• warn: message", "• warn: exception")]
    [TestCase(Error,       "message", true,  "• FAIL: message", "• FAIL: exception")]
    [TestCase(Critical,    "message", true,  "• CRIT: message", "• CRIT: exception")]
    [TestCase(-42,         "message", true,  "• ????: message", "• ????: exception")]
    // Empty message and exception
    //        Level        Message    Throw  Expected
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "",        true,  "• trac: exception")]
    [TestCase(Debug,       "",        true,  "• dbug: exception")]
    [TestCase(Information, "",        true,  "• info: exception")]
    [TestCase(Warning,     "",        true,  "• warn: exception")]
    [TestCase(Error,       "",        true,  "• FAIL: exception")]
    [TestCase(Critical,    "",        true,  "• CRIT: exception")]
    [TestCase(-42,         "",        true,  "• ????: exception")]
    // Null message and exception
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       null,      true,  "• trac: exception")]
    [TestCase(Debug,       null,      true,  "• dbug: exception")]
    [TestCase(Information, null,      true,  "• info: exception")]
    [TestCase(Warning,     null,      true,  "• warn: exception")]
    [TestCase(Error,       null,      true,  "• FAIL: exception")]
    [TestCase(Critical,    null,      true,  "• CRIT: exception")]
    [TestCase(-42,         null,      true,  "• ????: exception")]
    // Null entry formatter and exception
    //        Level        Message    Throw  Expected
    [TestCase(Trace,       "NEF",     true,  "• trac: exception")]
    [TestCase(Debug,       "NEF",     true,  "• dbug: exception")]
    [TestCase(Information, "NEF",     true,  "• info: exception")]
    [TestCase(Warning,     "NEF",     true,  "• warn: exception")]
    [TestCase(Error,       "NEF",     true,  "• FAIL: exception")]
    [TestCase(Critical,    "NEF",     true,  "• CRIT: exception")]
    [TestCase(-42,         "NEF",     true,  "• ????: exception")]
    public void Log_DebuggerAttached(
        LogLevel        logLevel,
        string?         message,
        bool            exceptional,
        params string[] expected)
    {
        using var h = new TestHarness(attached: true);

        var state     = h.Random.GetString();
        var eventId   = h.Random.Next();
        var exception = exceptional ? Thrown() : null;

        string Format(string s, Exception? e)
        {
            s.Should().BeSameAs(state);
            e.Should().BeSameAs(exception);
            return message!;
        }

        h.Logger.Log(
            logLevel, eventId, state, exception,
            formatter: message == "NEF" ? null! : Format
        );

        var expectedEntries = expected
            .Select(t => t.Replace("exception", exception?.ToString()))
            .Select(t => ((int) logLevel, h.Name, t + Environment.NewLine));

        ShouldBeEqual(h.Debugger.Entries, expectedEntries);
    }

    private void ShouldBeEqual(
        IEnumerable<(int, string, string)> actualEntries,
        IEnumerable<(int, string, string)> expectedEntries)
    {
        // BUG: Seeing nondeterministic behavior in Exception.ToString():
        // invoke .ToString() multiple times against the same exception, get
        // different line numbers in the stack trace.  To work around, replace
        // line numbers with 'X'.

        actualEntries   = actualEntries.Select(RemoveLineNumbers);
        expectedEntries = actualEntries.Select(RemoveLineNumbers);

        actualEntries.Should().Equal(expectedEntries);
    }

    private (int, string, string) RemoveLineNumbers((int, string, string) entry)
    {
        var (level, category, message) = entry;
        return (level, category, RemoveLineNumbers(message));
    }

    private string RemoveLineNumbers(string s)
        => LineNumberRegex.Replace(s, ":line X");

    private readonly Regex LineNumberRegex = new(
        @":line \d+$",
        RegexOptions.Multiline |
        RegexOptions.CultureInvariant
    );

    private class TestHarness : TestHarnessBase
    {
        public string                 Name     { get; }
        public DebuggerLoggerProvider Provider { get; }
        public DebuggerLogger         Logger   { get; }
        public TestDebugger           Debugger { get; }

        public TestHarness(bool attached)
        {
            Name     = Random.GetString(8);
            Provider = new();
            Debugger = new() { IsAttached = attached };
            Logger   = new(Provider, Name) { Debugger = Debugger };
        }
    }
}
