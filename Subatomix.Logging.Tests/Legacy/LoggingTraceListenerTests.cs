// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

#if NET6_0_OR_GREATER
#pragma warning disable CA1825 // Avoid unnecessary zero-length array allocations.
// Rationale: new[] { } used for readability below.
#endif

using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Logging.Fake;

namespace Subatomix.Logging.Legacy;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class LoggingTraceListenerTests
{
    [Test]
    public void LoggerFactory_Get()
    {
        new LoggingTraceListener()
            .LoggerFactory.Should().BeSameAs(NullLoggerFactory.Instance);
    }

    [Test]
    public void LoggerFactory_Set_NotNull()
    {
        var listener = new LoggingTraceListener();
        var provider = Mock.Of<ILoggerFactory>();

        listener.LoggerFactory = provider;
        listener.LoggerFactory.Should().BeSameAs(provider);
    }

    [Test]
    public void LoggerFactory_Set_Null()
    {
        new LoggingTraceListener()
            .Invoking(l => l.LoggerFactory = null!)
            .Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Test]
    public void IsThreadSafe_Get()
    {
        new LoggingTraceListener()
            .IsThreadSafe
            .Should().BeTrue();
    }

    [Test]
    public void TraceEvent_Id_On()
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for TraceEvent
        h.ExpectToUseLogger(TestTraceListenerName); // for Flush

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Error, 42);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,  0, "buffered text",  null),
            (LogLevel.Error, 42, "Message ID: 42", null)
        );
    }

    [Test]
    public void TraceEvent_Id_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Error, 42);
        h.Logger.Entries.Should().BeEmpty();
    }

    [Test]
    [TestCase(null,      ""       )]
    [TestCase("",        ""       )]
    [TestCase("message", "message")]
    public void TraceEvent_Message_On(string? message, string expected)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for TraceEvent
        h.ExpectToUseLogger(TestTraceListenerName); // for Flush

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Warning, 42, message);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,    0, "buffered text", null),
            (LogLevel.Warning, 42,  expected,       null)
        );
    }

    [Test]
    public void TraceEvent_Message_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Warning, 42, "message");
        h.Logger.Entries.Should().BeEmpty();
    }

    [Test]
    [TestCase(null,  null,                       ""       )]
    [TestCase(null,  new object[] { },           ""       )]
    [TestCase(null,  new object[] { "message" }, ""       )]
    [TestCase("",    null,                       ""       )]
    [TestCase("",    new object[] { },           ""       )]
    [TestCase("",    new object[] { "message" }, ""       )]
  //[TestCase("{0}", null,                       ""       )] // invalid inputs; throws
  //[TestCase("{0}", new object[] { },           ""       )] // invalid inputs; throws
    [TestCase("{0}", new object[] { "message" }, "message")]
    public void TraceEvent_Template_On(string? template, object?[]? args, string expected)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for TraceEvent
        h.ExpectToUseLogger(TestTraceListenerName); // for Flush

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Information, 42, template, args);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,        0, "buffered text", null),
            (LogLevel.Information, 42, expected,        null)
        );
    }

    [Test]
    public void TraceEvent_Template_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceEvent(TraceEventType.Information, 42, "{0} {1}", "the", "message");
        h.Logger.Entries.Should().BeEmpty();
    }

    [Test]
    [TestCase(null,             " {related:" + TestActivityId + "}")]
    [TestCase("",               " {related:" + TestActivityId + "}")]
    [TestCase("message", "message {related:" + TestActivityId + "}")]
    public void TraceTransfer_On(string? message, string expected)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for TraceTransfer
        h.ExpectToUseLogger(TestTraceListenerName); // for Flush

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceTransfer(42, message, new Guid(TestActivityId));

        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,        0, "buffered text", null),
            (LogLevel.Information, 42, expected,        null)
        );
    }

    [Test]
    public void TraceTransfer_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        var activityId = Guid.NewGuid();
        h.TraceSource.TraceTransfer(42, "message", activityId);
        h.Logger.Entries.Should().BeEmpty();
    }

    public static IEnumerable<TestCaseData> ObjectTestCases => new TestCaseData[]
    {
        new(null,        ""   ),
        new(Some("obj"), "obj"),
    };

    [Test]
    [TestCaseSource(nameof(ObjectTestCases))]
    public void TraceData_Single_On(object? obj, string expected)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for TraceData
        h.ExpectToUseLogger(TestTraceListenerName); // for Flush

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceData(TraceEventType.Verbose, 123, obj);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,   0, "buffered text", null),
            (LogLevel.Debug, 123, expected,        null)
        );
    }

    [Test]
    public void TraceData_Single_On_Exception()
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);

        var e = new InvalidOperationException("error message");
        h.TraceSource.TraceData(TraceEventType.Critical, 123, e);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Critical, 123, "", e)
        );
    }

    [Test]
    public void TraceData_Single_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        var obj = Some("obj");
        h.TraceSource.TraceData(TraceEventType.Verbose, 123, obj);
        h.Logger.Entries.Should().BeEmpty();
    }

    public static IEnumerable<TestCaseData> ArrayTestCases => new TestCaseData[]
    {
        new(null,                                         ""          ),
        new(new object?[] {                            }, ""          ),
        new(new object?[] { null                       }, ""          ),
        new(new object?[] { Some("objA")               }, "objA"      ),
        new(new object?[] { null,         Some("objB") }, ", objB"    ),
        new(new object?[] { Some("objA"), null         }, "objA, "    ),
        new(new object?[] { Some("objA"), Some("objB") }, "objA, objB"),
    };

    [Test]
    [TestCaseSource(nameof(ArrayTestCases))]
    public void TraceData_Array_On(object?[]? array, string expected)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);   // for Flush
        h.ExpectToUseLogger(TestTraceListenerName); // for TraceData

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceSource.TraceData(TraceEventType.Verbose, 123, array);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug,   0, "buffered text", null),
            (LogLevel.Debug, 123, expected,        null)
        );
    }

    [Test]
    public void TraceData_Array_On_SingleException()
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceSourceName);

        var e = new InvalidOperationException("error message");
        h.TraceSource.TraceData(TraceEventType.Error, 123, new object[] { e });
        h.Logger.Entries.Should().Equal(
            (LogLevel.Error, 123, "", e)
        );
    }

    [Test]
    public void TraceData_Array_Off()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        var o = new StringBuilder("an object");
        h.TraceSource.TraceData(TraceEventType.Verbose, 123, new object[] { o });
        h.Logger.Entries.Should().BeEmpty();
    }

    [Test]
    public void Close()
    {
        var listener = new LoggingTraceListener
        {
            LoggerFactory = Mock.Of<ILoggerFactory>()
        };

        listener.Close();

        listener.LoggerFactory.Should().BeSameAs(NullLoggerFactory.Instance);

        listener
            .Invoking(l => l.Write("something"))
            .Should().NotThrow();
    }

    [Test]
    public void Dispose()
    {
        var listener = new LoggingTraceListener
        {
            LoggerFactory = Mock.Of<ILoggerFactory>()
        };

        listener.Dispose();

        listener.LoggerFactory.Should().BeSameAs(NullLoggerFactory.Instance);

        listener
            .Invoking(l => l.Write("something"))
            .Should().Throw<ObjectDisposedException>();
    }

    [Test]
    [TestCase(null,      ""       )]
    [TestCase("",        ""       )]
    [TestCase("message", "message")]
    public void Fail_Message(string? message, string expected)
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off); // to show this doesn't affect Fail
        h.ExpectToUseLogger(TestTraceListenerName);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceListener.Fail(message);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug, 0, "buffered text", null),
            (LogLevel.Error, 0, expected,        null)
        );
    }

    [Test]
    [TestCase(null,      null,     ""              )]
    [TestCase(null,      "",       ""              )]
    [TestCase(null,      "detail", "detail"        )]
    [TestCase("",        null,     ""              )]
    [TestCase("",        "",       ""              )]
    [TestCase("",        "detail", "detail"        )]
    [TestCase("message", null,     "message"       )]
    [TestCase("message", "",       "message"       )]
    [TestCase("message", "detail", "message detail")]
    public void Fail_MessageWithDetail(string? message, string? detail, string expected)
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off); // to show this doesn't affect Fail
        h.ExpectToUseLogger(TestTraceListenerName);

        h.TraceListener.Write("buffered text");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceListener.Fail(message, detail);
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug, 0, "buffered text", null),
            (LogLevel.Error, 0, expected,        null)
        );
    }

    [Test]
    public void Write()
    {
        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off); // to show this doesn't affect Write
        h.ExpectToUseLogger(TestTraceListenerName);

        h.TraceListener.Write("a");
        h.TraceListener.Write(null);
        h.TraceListener.Write("b");
        h.TraceListener.Write("");
        h.TraceListener.Write("c");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceListener.Flush();
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug, 0, "abc", null)
        );
    }

    [Test]
    public void WriteLine()
    {
        var nl = Environment.NewLine;

        using var h = new TestHarness();
        h.SetUpFilter(SourceLevels.Off); // to show this doesn't affect Write
        h.ExpectToUseLogger(TestTraceListenerName);

        h.TraceListener.WriteLine("a");
        h.TraceListener.WriteLine(null);
        h.TraceListener.WriteLine("b");
        h.TraceListener.WriteLine("");
        h.TraceListener.WriteLine("c");
        h.Logger.Entries.Should().BeEmpty();

        h.TraceListener.Flush();
        h.Logger.Entries.Should().Equal(
            (LogLevel.Debug, 0, Invariant($"a{nl}{nl}b{nl}{nl}c{nl}"), null)
        );
    }

    [Test]
    public void SpecialCases_NoSourceName(
        [Values(null, "")] string? source)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(TestTraceListenerName);

        var type       = TraceEventType.Error;
        var activityId = Guid.NewGuid();

        h.TraceListener.TraceEvent   (Context(), source!, type, 42);
        h.TraceListener.TraceEvent   (Context(), source!, type, 42, "message");
        h.TraceListener.TraceEvent   (Context(), source!, type, 42, "{0}", "arg");
        h.TraceListener.TraceData    (Context(), source!, type, 42, Some("object"));
        h.TraceListener.TraceData    (Context(), source!, type, 42, Some("objectA"), Some("objectB"));
        h.TraceListener.TraceTransfer(Context(), source!,       42, "message", activityId);

        h.Logger.Entries.Should().Equal(
            (LogLevel.Error,       42, "Message ID: 42",                               null),
            (LogLevel.Error,       42, "message",                                      null),
            (LogLevel.Error,       42, "arg",                                          null),
            (LogLevel.Error,       42, "object",                                       null),
            (LogLevel.Error,       42, "objectA, objectB",                             null),
            (LogLevel.Information, 42, Invariant($"message {{related:{activityId}}}"), null)
        );
    }

    [Test]
    public void SpecialCases_NoSourceName_NoListenerName(
        [Values(null, "")] string? source,
        [Values(null, "")] string? listenerName)
    {
        using var h = new TestHarness();
        h.ExpectToUseLogger(LoggingTraceListener.DefaultLoggerCategory);
        h.TraceListener.Name = listenerName!;

        var type       = TraceEventType.Error;
        var activityId = Guid.NewGuid();

        h.TraceListener.TraceEvent   (Context(), source!, type, 42);
        h.TraceListener.TraceEvent   (Context(), source!, type, 42, "message");
        h.TraceListener.TraceEvent   (Context(), source!, type, 42, "{0}", "arg");
        h.TraceListener.TraceData    (Context(), source!, type, 42, Some("object"));
        h.TraceListener.TraceData    (Context(), source!, type, 42, Some("objectA"), Some("objectB"));
        h.TraceListener.TraceTransfer(Context(), source!,       42, "message", activityId);

        h.Logger.Entries.Should().Equal(
            (LogLevel.Error,       42, "Message ID: 42",                               null),
            (LogLevel.Error,       42, "message",                                      null),
            (LogLevel.Error,       42, "arg",                                          null),
            (LogLevel.Error,       42, "object",                                       null),
            (LogLevel.Error,       42, "objectA, objectB",                             null),
            (LogLevel.Information, 42, Invariant($"message {{related:{activityId}}}"), null)
        );
    }

    private static TraceEventCache Context()
        => new();

    private static object Some(string s)
        => new StringBuilder(s);

    private const string
        TestActivityId        = "9b143346-3044-4e8c-bd33-db005b39a350",
        TestTraceSourceName   = "TestTraceSource",
        TestTraceListenerName = "TestTraceListener";

    private class TestHarness : TestHarnessBase
    {
        public TraceSource          TraceSource   { get; }
        public LoggingTraceListener TraceListener { get; }
        public TraceEventCache      TraceContext  { get; }

        public ILoggerFactory       LoggerFactory { get; }
        public FakeLogger           Logger        { get; }

        public TestHarness()
        {
            TraceSource   = new TraceSource(TestTraceSourceName, SourceLevels.All);
            TraceListener = new LoggingTraceListener();
            TraceContext  = new TraceEventCache();

            LoggerFactory = Mocks.Create<ILoggerFactory>().Object;
            Logger        = new FakeLogger();

            TraceSource.Listeners.Clear();
            TraceSource.Listeners.Add(TraceListener);

            TraceListener.Name          = TestTraceListenerName;
            TraceListener.LoggerFactory = LoggerFactory;
        }

        public void SetUpFilter(SourceLevels level)
        {
            TraceListener.Filter = new EventTypeFilter(level);
        }

        public void ExpectToUseLogger(string categoryName)
        {
            Mock.Get(LoggerFactory)
                .Setup(f => f.CreateLogger(categoryName))
                .Returns(Logger)
                .Verifiable();
        }
    }
}
