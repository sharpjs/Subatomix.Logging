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

using Microsoft.Extensions.Logging;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging;

using static LogLevel;

[TestFixture]
public class OperationScopeInitiatorTests
{
    #region Begin

    [Test]
    public void Begin()
    {
        var logger   = new TestLogger();
        var logLevel = Any.LogLevelExceptNone();
        var name     = Any.GetString();

        var initiator = new OperationScopeInitiator(logger, logLevel, name);

        using var scope = initiator.Begin();

        scope            .Should().NotBeNull();
        scope.IsCompleted.Should().BeFalse();
        scope.Logger     .Should().BeSameAs(logger);
        scope.LogLevel   .Should().Be(logLevel);
        scope.Name       .Should().BeSameAs(name);
    }

    #endregion
    #region Do
    #region Do_Op

    [Test]
    public void Do_Op_NullAction()
    {
        var op = default(Action);

        new TestLogger()
            .Invoking(l => l.Operation("a").Do(op!))
            .Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public void Do_Op_Normal()
    {
        var logger = new TestLogger();

        void Op()
        {
            AssertDoStarted(logger, "a");
        }

        logger.Operation("a").Do(Op);

        AssertDoCompleted(logger, "a");
    }

    [Test]
    public void Do_Op_Exception()
    {
        var logger = new TestLogger();

        static void Op()
            => throw new Exception();

        logger
            .Invoking(l => l.Operation("a").Do(Op))
            .Should().Throw<Exception>()
            .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // Do_Op
    #region Do_OpWithArg

    [Test]
    public void Do_OpWithArg_NullAction()
    {
        var arg = new Arg();
        var op  = default(Action<Arg>);

        new TestLogger()
            .Invoking(l => l.Operation("a").Do(arg, op!))
            .Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public void Do_OpWithArg_Normal()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        void Op(Arg a)
        {
            a.Should().BeSameAs(arg);

            AssertDoStarted(logger, "a");
        }

        logger.Operation("a").Do(arg, Op);

        AssertDoCompleted(logger, "a");
    }

    [Test]
    public void Do_OpWithArg_Exception()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        static void Op(Arg a)
            => throw new Exception();

        logger
            .Invoking(l => l.Operation("a").Do(arg, Op))
            .Should().Throw<Exception>()
            .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // Do_OpWithArg
    #region Do_OpWithResult

    [Test]
    public void Do_OpWithResult_NullAction()
    {
        var op = default(Func<Result>);

        new TestLogger()
            .Invoking(l => l.Operation("a").Do(op!))
            .Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public void Do_OpWithResult_Normal()
    {
        var logger   = new TestLogger();
        var expected = new Result();

        Result Op()
        {
            AssertDoStarted(logger, "a");

            return expected;
        }

        var result = logger.Operation("a").Do(Op);

        AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void Do_OpWithResult_Exception()
    {
        var logger = new TestLogger();

        static Result Op()
            => throw new Exception();

        logger
            .Invoking(l => l.Operation("a").Do(Op))
            .Should().Throw<Exception>()
            .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // Do_OpWithResult
    #region Do_OpWithArgAndResult

    [Test]
    public void Do_OpWithArgAndResult_NullAction()
    {
        var arg = new Arg();
        var op  = default(Func<Arg, Result>);

        new TestLogger()
            .Invoking(l => l.Operation("a").Do(arg, op!))
            .Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public void Do_OpWithArgAndResult_Normal()
    {
        var logger   = new TestLogger();
        var arg      = new Arg();
        var expected = new Result();

        Result Op(Arg a)
        {
            a.Should().BeSameAs(arg);

            AssertDoStarted(logger, "a");

            return expected;
        }

        var result = logger.Operation("a").Do(arg, Op);

        AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void Do_OpWithArgAndResult_Exception()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        static Result Op(Arg a)
            => throw new Exception();

        logger
            .Invoking(l => l.Operation("a").Do(arg, Op))
            .Should().Throw<Exception>()
            .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // Do_OpWithArgAndResult
    #endregion // Do
    #region DoAsync
    #region DoAsync_Op

    [Test]
    public async Task DoAsync_Op_NullAction()
    {
        var op = default(Func<Task>);

        await new TestLogger()
            .Awaiting(l => l.Operation("a").DoAsync(op!))
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public async Task DoAsync_Op_Normal()
    {
        var logger = new TestLogger();

        Task Op()
        {
            AssertDoStarted(logger, "a");

            return Task.CompletedTask;
        }

        await logger.Operation("a").DoAsync(Op);

        AssertDoCompleted(logger, "a");
    }

    [Test]
    public async Task DoAsync_Op_Exception()
    {
        var logger = new TestLogger();

        static Task Op()
            => throw new Exception();

        (await logger
            .Awaiting(l => l.Operation("a").DoAsync(Op))
            .Should().ThrowAsync<Exception>()
        )   .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // DoAsync_Op
    #region DoAsync_OpWithArg

    [Test]
    public async Task DoAsync_OpWithArg_NullAction()
    {
        var arg = new Arg();
        var op  = default(Func<Arg, Task>);

        await new TestLogger()
            .Awaiting(l => l.Operation("a").DoAsync(arg, op!))
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public async Task DoAsync_OpWithArg_Normal()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        Task Op(Arg a)
        {
            a.Should().BeSameAs(arg);

            AssertDoStarted(logger, "a");

            return Task.CompletedTask;
        }

        await logger.Operation("a").DoAsync(arg, Op);

        AssertDoCompleted(logger, "a");
    }

    [Test]
    public async Task DoAsync_OpWithArg_Exception()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        static Task Op(Arg a)
            => throw new Exception();

        (await logger
            .Awaiting(l => l.Operation("a").DoAsync(arg, Op))
            .Should().ThrowAsync<Exception>()
        )   .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // DoAsync_OpWithArg
    #region DoAsync_OpWithResult

    [Test]
    public async Task DoAsync_OpWithResult_NullAction()
    {
        var op = default(Func<Task<Result>>);

        await new TestLogger()
            .Awaiting(l => l.Operation("a").DoAsync(op!))
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public async Task DoAsync_OpWithResult_Normal()
    {
        var logger   = new TestLogger();
        var expected = new Result();

        Task<Result> Op()
        {
            AssertDoStarted(logger, "a");

            return Task.FromResult(expected);
        }

        var result = await logger.Operation("a").DoAsync(Op);

        AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public async Task DoAsync_OpWithResult_Exception()
    {
        var logger = new TestLogger();

        static Task<Result> Op()
            => throw new Exception();

        (await logger
            .Awaiting(l => l.Operation("a").DoAsync(Op))
            .Should().ThrowAsync<Exception>()
        )   .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // DoAsync_OpWithResult
    #region DoAsync_OpWithArgAndResult

    [Test]
    public async Task DoAsync_OpWithArgAndResult_NullAction()
    {
        var arg = new Arg();
        var op  = default(Func<Arg, Task<Result>>);

        await new TestLogger()
            .Awaiting(l => l.Operation("a").DoAsync(arg, op!))
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
    }

    [Test]
    public async Task DoAsync_OpWithArgAndResult_Normal()
    {
        var logger   = new TestLogger();
        var arg      = new Arg();
        var expected = new Result();

        Task<Result> Op(Arg a)
        {
            a.Should().BeSameAs(arg);

            AssertDoStarted(logger, "a");

            return Task.FromResult(expected);
        }

        var result = await logger.Operation("a").DoAsync(arg, Op);

        AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public async Task DoAsync_OpWithArgAndResult_Exception()
    {
        var logger = new TestLogger();
        var arg    = new Arg();

        static Task<Result> Op(Arg a)
            => throw new Exception();

        (await logger
            .Awaiting(l => l.Operation("a").DoAsync(arg, Op))
            .Should().ThrowAsync<Exception>()
        )   .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);
    }

    #endregion // DoAsync_OpWithArgAndResult
    #endregion // DoAsync

    private void AssertDoStarted(TestLogger logger, string name)
    {
        logger.Entries.Should().HaveCount(1);

        var e0 = logger.Entries[0];

        e0.LogLevel .Should().Be(Information);
        e0.Message  .Should().Be($"{name}: Starting");
        e0.Exception.Should().BeNull();
    }

    private void AssertDoCompleted(TestLogger logger, string name)
    {
        logger.Entries.Should().HaveCount(2);
 
        var e1 = logger.Entries[1];

        e1.LogLevel .Should().Be(Information);
        e1.Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d\d\ds\]$");
        e1.Exception.Should().BeNull();
    }

    private void AssertDoCompleted(TestLogger logger, string name, Exception exception)
    {
        logger.Entries.Should().HaveCount(3);
 
        var e1 = logger.Entries[1];
        var e2 = logger.Entries[2];

        e1.LogLevel .Should().Be(Error);
        e1.Message  .Should().BeNullOrEmpty();
        e1.Exception.Should().BeSameAs(exception);
 
        e2.LogLevel .Should().Be(Information);
        e2.Message  .Should().MatchRegex($@"^{name}: Completed \[\d+\.\d\d\ds\] \[EXCEPTION\]$");
        e2.Exception.Should().BeNull();
    }

    private sealed class Arg    { }
    private sealed class Result { }
}
