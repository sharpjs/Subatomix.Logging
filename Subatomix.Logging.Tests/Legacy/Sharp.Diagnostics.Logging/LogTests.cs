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

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Logging.Testing;

namespace Sharp.Diagnostics.Logging;

using static ExceptionTestHelpers;
using static LogLevel;

[TestFixture]
[NonParallelizable] // because Log uses a static ILogger
public class LogTests
{
    #region Properties / Misc

    [OneTimeSetUp]
    public static void OneTimeSetUp()
    {
        // Must test this in OneTimeSetUp, because SetUp overwrites the property
        Log.Logger.Should().BeOfType<NullLogger>();
    }

    [SetUp]
    public void SetUp()
    {
        Log.Logger = new TestLogger();
    }

    [Test]
    public void Logger_Get()
    {
        Log.Logger.Should().BeOfType<TestLogger>();
    }

    [Test]
    public void Logger_Set()
    {
        var logger = new TestLogger();

        Log.Logger = logger;

        Log.Logger.Should().BeSameAs(logger);
    }

    [Test]
    public void Logger_Set_Null()
    {
        Invoking(() => Log.Logger = null!)
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Flush()
    {
        Log.Flush();
    }

    [Test]
    public void Close()
    {
        Log.Close();
    }
    #endregion
    #region Critical

    [Test]
    public void Critical_Message()
    {
        Log.Critical("a");
        Entries.Should().Equal((Critical, 0, "a", null));
    }

    [Test]
    public void Critical_IdAndMessage()
    {
        Log.Critical(42, "a");
        Entries.Should().Equal((Critical, 42, "a", null));
    }

    [Test]
    public void Critical_Format()
    {
        Log.Critical("<{0}>", "a");
        Entries.Should().Equal((Critical, 0, "<a>", null));
    }

    [Test]
    public void Critical_IdAndFormat()
    {
        Log.Critical(42, "<{0}>", "a");
        Entries.Should().Equal((Critical, 42, "<a>", null));
    }

    [Test]
    public void Critical_Exception()
    {
        var e = new Exception("a");
        Log.Critical(e);
        Entries.Should().Equal((Critical, 0, "", e));
    }

    [Test]
    public void Critical_IdAndException()
    {
        var e = new Exception("a");
        Log.Critical(42, e);
        Entries.Should().Equal((Critical, 42, "", e));
    }

    #endregion
    #region Error

    [Test]
    public void Error_Message()
    {
        Log.Error("a");
        Entries.Should().Equal((Error, 0, "a", null));
    }

    [Test]
    public void Error_IdAndMessage()
    {
        Log.Error(42, "a");
        Entries.Should().Equal((Error, 42, "a", null));
    }

    [Test]
    public void Error_Format()
    {
        Log.Error("<{0}>", "a");
        Entries.Should().Equal((Error, 0, "<a>", null));
    }

    [Test]
    public void Error_IdAndFormat()
    {
        Log.Error(42, "<{0}>", "a");
        Entries.Should().Equal((Error, 42, "<a>", null));
    }

    [Test]
    public void Error_Exception()
    {
        var e = new Exception("a");
        Log.Error(e);
        Entries.Should().Equal((Error, 0, "", e));
    }

    [Test]
    public void Error_IdAndException()
    {
        var e = new Exception("a");
        Log.Error(42, e);
        Entries.Should().Equal((Error, 42, "", e));
    }

    #endregion
    #region Warning

    [Test]
    public void Warning_Message()
    {
        Log.Warning("a");
        Entries.Should().Equal((Warning, 0, "a", null));
    }

    [Test]
    public void Warning_IdAndMessage()
    {
        Log.Warning(42, "a");
        Entries.Should().Equal((Warning, 42, "a", null));
    }

    [Test]
    public void Warning_Format()
    {
        Log.Warning("<{0}>", "a");
        Entries.Should().Equal((Warning, 0, "<a>", null));
    }

    [Test]
    public void Warning_IdAndFormat()
    {
        Log.Warning(42, "<{0}>", "a");
        Entries.Should().Equal((Warning, 42, "<a>", null));
    }

    [Test]
    public void Warning_Exception()
    {
        var e = new Exception("a");
        Log.Warning(e);
        Entries.Should().Equal((Warning, 0, "", e));
    }

    [Test]
    public void Warning_IdAndException()
    {
        var e = new Exception("a");
        Log.Warning(42, e);
        Entries.Should().Equal((Warning, 42, "", e));
    }

    #endregion
    #region Information

    [Test]
    public void Information_Message()
    {
        Log.Information("a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Information_IdAndMessage()
    {
        Log.Information(42, "a");
        Entries.Should().Equal((Information, 42, "a", null));
    }

    [Test]
    public void Information_Format()
    {
        Log.Information("<{0}>", "a");
        Entries.Should().Equal((Information, 0, "<a>", null));
    }

    [Test]
    public void Information_IdAndFormat()
    {
        Log.Information(42, "<{0}>", "a");
        Entries.Should().Equal((Information, 42, "<a>", null));
    }

    [Test]
    public void Information_Exception()
    {
        var e = new Exception("a");
        Log.Information(e);
        Entries.Should().Equal((Information, 0, "", e));
    }

    [Test]
    public void Information_IdAndException()
    {
        var e = new Exception("a");
        Log.Information(42, e);
        Entries.Should().Equal((Information, 42, "", e));
    }

    #endregion
    #region Verbose

    [Test]
    public void Verbose_Message()
    {
        Log.Verbose("a");
        Entries.Should().Equal((Debug, 0, "a", null));
    }

    [Test]
    public void Verbose_IdAndMessage()
    {
        Log.Verbose(42, "a");
        Entries.Should().Equal((Debug, 42, "a", null));
    }

    [Test]
    public void Verbose_Format()
    {
        Log.Verbose("<{0}>", "a");
        Entries.Should().Equal((Debug, 0, "<a>", null));
    }

    [Test]
    public void Verbose_IdAndFormat()
    {
        Log.Verbose(42, "<{0}>", "a");
        Entries.Should().Equal((Debug, 42, "<a>", null));
    }

    [Test]
    public void Verbose_Exception()
    {
        var e = new Exception("a");
        Log.Verbose(e);
        Entries.Should().Equal((Debug, 0, "", e));
    }

    [Test]
    public void Verbose_IdAndException()
    {
        var e = new Exception("a");
        Log.Verbose(42, e);
        Entries.Should().Equal((Debug, 42, "", e));
    }

    #endregion
    #region Start

    [Test]
    public void Start_Message()
    {
        Log.Start("a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Start_IdAndMessage()
    {
        Log.Start(42, "a");
        Entries.Should().Equal((Information, 42, "a", null));
    }

    [Test]
    public void Start_Format()
    {
        Log.Start("<{0}>", "a");
        Entries.Should().Equal((Information, 0, "<a>", null));
    }

    [Test]
    public void Start_IdAndFormat()
    {
        Log.Start(42, "<{0}>", "a");
        Entries.Should().Equal((Information, 42, "<a>", null));
    }

    [Test]
    public void Start_Exception()
    {
        var e = new Exception("a");
        Log.Start(e);
        Entries.Should().Equal((Information, 0, "", e));
    }

    [Test]
    public void Start_IdAndException()
    {
        var e = new Exception("a");
        Log.Start(42, e);
        Entries.Should().Equal((Information, 42, "", e));
    }

    #endregion
    #region Stop

    [Test]
    public void Stop_Message()
    {
        Log.Stop("a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Stop_IdAndMessage()
    {
        Log.Stop(42, "a");
        Entries.Should().Equal((Information, 42, "a", null));
    }

    [Test]
    public void Stop_Format()
    {
        Log.Stop("<{0}>", "a");
        Entries.Should().Equal((Information, 0, "<a>", null));
    }

    [Test]
    public void Stop_IdAndFormat()
    {
        Log.Stop(42, "<{0}>", "a");
        Entries.Should().Equal((Information, 42, "<a>", null));
    }

    [Test]
    public void Stop_Exception()
    {
        var e = new Exception("a");
        Log.Stop(e);
        Entries.Should().Equal((Information, 0, "", e));
    }

    [Test]
    public void Stop_IdAndException()
    {
        var e = new Exception("a");
        Log.Stop(42, e);
        Entries.Should().Equal((Information, 42, "", e));
    }

    #endregion
    #region Suspend

    [Test]
    public void Suspend_Message()
    {
        Log.Suspend("a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Suspend_IdAndMessage()
    {
        Log.Suspend(42, "a");
        Entries.Should().Equal((Information, 42, "a", null));
    }

    [Test]
    public void Suspend_Format()
    {
        Log.Suspend("<{0}>", "a");
        Entries.Should().Equal((Information, 0, "<a>", null));
    }

    [Test]
    public void Suspend_IdAndFormat()
    {
        Log.Suspend(42, "<{0}>", "a");
        Entries.Should().Equal((Information, 42, "<a>", null));
    }

    [Test]
    public void Suspend_Exception()
    {
        var e = new Exception("a");
        Log.Suspend(e);
        Entries.Should().Equal((Information, 0, "", e));
    }

    [Test]
    public void Suspend_IdAndException()
    {
        var e = new Exception("a");
        Log.Suspend(42, e);
        Entries.Should().Equal((Information, 42, "", e));
    }

    #endregion
    #region Resume

    [Test]
    public void Resume_Message()
    {
        Log.Resume("a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Resume_IdAndMessage()
    {
        Log.Resume(42, "a");
        Entries.Should().Equal((Information, 42, "a", null));
    }

    [Test]
    public void Resume_Format()
    {
        Log.Resume("<{0}>", "a");
        Entries.Should().Equal((Information, 0, "<a>", null));
    }

    [Test]
    public void Resume_IdAndFormat()
    {
        Log.Resume(42, "<{0}>", "a");
        Entries.Should().Equal((Information, 42, "<a>", null));
    }

    [Test]
    public void Resume_Exception()
    {
        var e = new Exception("a");
        Log.Resume(e);
        Entries.Should().Equal((Information, 0, "", e));
    }

    [Test]
    public void Resume_IdAndException()
    {
        var e = new Exception("a");
        Log.Resume(42, e);
        Entries.Should().Equal((Information, 42, "", e));
    }

    #endregion
    #region Operation / Correlation

    [Test]
    public void ActivityId_Default()
    {
        Log.ActivityId.Should().BeEmpty();
    }

    [Test]
    public void ActivityId_NotDefault()
    {
        try
        {
            var id = Guid.NewGuid();

            SD.Trace.CorrelationManager.ActivityId = id;

            Log.ActivityId.Should().Be(id);
        }
        finally
        {
            SD.Trace.CorrelationManager.ActivityId = default;
        }
    }

    [Test]
    public void GetOperationStack_Empty()
    {
        Log.GetOperationStack().Should().BeEmpty();
    }

    [Test]
    public void GetOperationStack_NotEmpty()
    {
        var stack = SD.Trace.CorrelationManager.LogicalOperationStack;
        var id0   = Any.GetString();
        var id1   = Any.GetString();

        try
        {
            stack.Push(id0);
            stack.Push(id1);

            Log.GetOperationStack().Should().Equal(id1, id0);
        }
        finally
        {
            stack.Clear();
        }
    }

    [Test]
    public void Operation()
    {
        using var operation = Log.Operation();

        operation.Should().BeOfType<TraceOperation>();
        operation.Name.Should().Be(nameof(Operation)); // testing [CallerMemberName]
    }

    #endregion
    #region Do / DoAsync

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoOpTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            Log.Logger = logger;
            Log.Do(name, action);
            return result;
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoOpWithResultTests : DoTests
    {
        private protected override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            Log.Logger = logger;
            return Log.Do(name, action.Returning(result));
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoAsyncOpTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            Log.Logger = logger;
            await Log.DoAsync(name, action);
            return result;
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoAsyncOpWithResultTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            Log.Logger = logger;
            await Log.DoAsync(name, action.Returning(result));
            return result;
        }
    }

    #endregion // Do / DoAsync
    #region Transfer

    [Test]
    public void Transfer_MessageAndActivityId()
    {
        var id = Guid.NewGuid();
        Log.Transfer("a", id);
        Entries.Should().Equal((Information, 0, $"a {{related:{id}}}", null));
    }

    [Test]
    public void Transfer_IdAndMessageAndActivityId()
    {
        var id = Guid.NewGuid();
        Log.Transfer(42, "a", id);
        Entries.Should().Equal((Information, 42, $"a {{related:{id}}}", null));
    }

    #endregion
    #region Event

    [Test]
    public void Event_Id()
    {
        Log.Event(TraceEventType.Verbose, 42);
        Entries.Should().Equal((Debug, 42, "Message ID: 42", null));
    }

    [Test]
    public void Event_Message()
    {
        Log.Event(TraceEventType.Information, "a");
        Entries.Should().Equal((Information, 0, "a", null));
    }

    [Test]
    public void Event_IdAndMessage()
    {
        Log.Event(TraceEventType.Warning, 42, "a");
        Entries.Should().Equal((Warning, 42, "a", null));
    }

    [Test]
    public void Event_Format()
    {
        Log.Event(TraceEventType.Error, "<{0}>", "a");
        Entries.Should().Equal((Error, 0, "<a>", null));
    }

    [Test]
    public void Event_IdAndFormat()
    {
        Log.Event(TraceEventType.Critical, 42, "<{0}>", "a");
        Entries.Should().Equal((Critical, 42, "<a>", null));
    }

    #endregion
    #region Data

    private static object Some(string s)
        => new StringBuilder(s);

    public static IEnumerable<TestCaseData> ObjectTestCases => new TestCaseData[]
    {
        new(null,        ""   ),
        new(Some("obj"), "obj"),
    };

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
    [TestCaseSource(nameof(ObjectTestCases))]
    public void Data_Object(object? obj, string expected)
    {
        Log.Data(TraceEventType.Verbose, obj);
        Entries.Should().Equal((Debug, 0, expected, null));
    }

    [Test]
    [TestCaseSource(nameof(ObjectTestCases))]
    public void Data_IdAndObject(object? obj, string expected)
    {
        Log.Data(TraceEventType.Information, 42, obj);
        Entries.Should().Equal((Information, 42, expected, null));
    }

    [Test]
    [TestCaseSource(nameof(ArrayTestCases))]
    public void Data_ObjectArray(object?[]? array, string expected)
    {
        Log.Data(TraceEventType.Warning, array);
        Entries.Should().Equal((Warning, 0, expected, null));
    }

    [Test]
    [TestCaseSource(nameof(ArrayTestCases))]
    public void Data_IdAndObjectArray(object?[]? array, string expected)
    {
        Log.Data(TraceEventType.Error, 42, array);
        Entries.Should().Equal((Error, 42, expected, null));
    }

    #endregion
    #region Event Handlers

    [Test]
    public void LogAllThrownExceptions()
    {
        var exception = null as Exception;

        try
        {
            // Set twice to test no-change path
            Log.LogAllThrownExceptions = true;
            Log.LogAllThrownExceptions.Should().BeTrue();
            Log.LogAllThrownExceptions = true;
            Log.LogAllThrownExceptions.Should().BeTrue();

            exception = Thrown("*YEP*");
        }
        finally
        {
            // Set twice to test no-change path
            Log.LogAllThrownExceptions = false;
            Log.LogAllThrownExceptions.Should().BeFalse();
            Log.LogAllThrownExceptions = false;
            Log.LogAllThrownExceptions.Should().BeFalse();

            _ = Thrown("*NOPE*");
        }

        exception.Should().NotBeNull();

        Entries.Should().Equal((Debug, 0, "An exception was thrown.", exception));
    }

    [Test]
    public void LogAllThrownExceptions_NullEventArgs()
    {
        // This should not ever happen, but test for it anyway.
        Log.SimulateFirstChanceException(null!);

        Entries.Should().BeEmpty();
    }

    [Test]
    public void LogAllThrownExceptions_NullException()
    {
        // This should not ever happen, but test for it anyway.
        Log.SimulateFirstChanceException(new(null!));

        Entries.Should().BeEmpty();
    }

    [Test]
    public void LogAllThrownExceptions_SecondaryException()
    {
        // Simulate unhandled secondary exception
        Log.Logger = Mock.Of<ILogger>(MockBehavior.Strict);

        Log.SimulateFirstChanceException(new(Thrown()));
    }

    [Test]
    public void LogAllThrownExceptions_Reentrant()
    {
        var original  = Thrown("Original exception.");
        var secondary = Thrown("Secondary exception.");

        Log.Logger = Mock.Of<ILogger>(MockBehavior.Strict);

        // Simulate reentrancy due to secondary exception
        Mock.Get(Log.Logger)
            .Setup(l => l.Log(
                Debug, 0, It.IsAny<It.IsAnyType>(),
                original, It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ))
            .Callback(() => Log.SimulateFirstChanceException(new(secondary)))
            .Verifiable();

        Log.SimulateFirstChanceException(new(original));

        Mock.Get(Log.Logger).Verify();
    }

    [Test]
    public void CloseOnExit_UnhandledException_Terminating()
    {
        var exception = Thrown();

        try
        {
            // Set twice to test no-change path
            Log.CloseOnExit = true;
            Log.CloseOnExit.Should().BeTrue();
            Log.CloseOnExit = true;
            Log.CloseOnExit.Should().BeTrue();

            Log.SimulateUnhandledException(new(exception, isTerminating: true));
        }
        finally
        {
            // Set twice to test no-change path
            Log.CloseOnExit = false;
            Log.CloseOnExit.Should().BeFalse();
            Log.CloseOnExit = false;
            Log.CloseOnExit.Should().BeFalse();
        }

        Entries.Should().Equal(
            (Critical, 0, "Terminating due to an unhandled exception of type System.ApplicationException.", null),
            (Critical, 0, "", exception)
        );
    }

    [Test]
    public void CloseOnExit_UnhandledException_Nonterminating()
    {
        // Documentation seems to indicate that this case could only have
        // occured in .NET Framework 1.0 and 1.1.  Test for it anyway.

        var exception = new ApplicationException();

        try
        {
            Log.CloseOnExit = true;
            Log.CloseOnExit.Should().BeTrue();

            Log.SimulateUnhandledException(new(exception, isTerminating: false));
        }
        finally
        {
            Log.CloseOnExit = false;
            Log.CloseOnExit.Should().BeFalse();
        }

        Entries.Should().Equal(
            (Error, 0, "Unhandled exception of type System.ApplicationException. Execution will continue.", null),
            (Error, 0, "", exception)
        );
    }

    [Test]
    public void CloseOnExit_UnhandledException_NullEventArgs()
    {
        // This should not ever happen, but test for it anyway.
        Log.SimulateUnhandledException(null!);

        Entries.Should().BeEmpty();
    }

    [Test]
    public void CloseOnExit_UnhandledException_NullException()
    {
        // This should not ever happen, but test for it anyway.
        Log.SimulateUnhandledException(new(null!, isTerminating: true));

        Entries.Should().BeEmpty();
    }

    [Test]
    public void CloseOnExit_UnhandledException_SecondaryException()
    {
        // Set up logger to throw a secondary exception
        Log.Logger = Mock.Of<ILogger>(MockBehavior.Strict);

        Log.SimulateUnhandledException(new(Thrown(), isTerminating: true));
    }

    [Test]
    public void CloseOnExit_DomainUnload()
    {
        var domain = AppDomain.CurrentDomain;

        try
        {
            Log.CloseOnExit = true;
            Log.SimulateDomainUnload(domain);
        }
        finally
        {
            Log.CloseOnExit = false;
        }

        Entries.Should().Equal(
            (Information, 0, $"The AppDomain '{domain.FriendlyName}' is unloading.", null)
        );
    }

    [Test]
    public void CloseOnExit_DomainUnload_UnknownSender()
    {
        // This should not ever happen, but test for it anyway.
        var domain = new object();

        try
        {
            Log.CloseOnExit = true;
            Log.SimulateDomainUnload(domain);
        }
        finally
        {
            Log.CloseOnExit = false;
        }

        Entries.Should().Equal(
            (Information, 0, "The AppDomain 'unknown' is unloading.", null)
        );
    }

    [Test]
    public void CloseOnExit_ProcessExit()
    {
        var domain = AppDomain.CurrentDomain;

        try
        {
            Log.CloseOnExit = true;
            Log.SimulateProcessExit(domain);
        }
        finally
        {
            Log.CloseOnExit = false;
        }

        Entries.Should().Equal(
            (Information, 0, $"The parent process of AppDomain '{domain.FriendlyName}' is exiting.", null)
        );
    }

    [Test]
    public void CloseOnExit_ProcessExit_UnknownSender()
    {
        // This should not ever happen, but test for it anyway.
        var domain = new object();

        try
        {
            Log.CloseOnExit = true;
            Log.SimulateProcessExit(domain);
        }
        finally
        {
            Log.CloseOnExit = false;
        }

        Entries.Should().Equal(
            (Information, 0, "The parent process of AppDomain 'unknown' is exiting.", null)
        );
    }

    #endregion

    public IReadOnlyList<(LogLevel LogLevel, EventId Id, string Message, Exception? Exception)>
        Entries => ((TestLogger) Log.Logger).Entries2;
}
