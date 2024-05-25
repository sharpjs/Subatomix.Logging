// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;

namespace Subatomix.Logging.Testing;

public abstract class DoTests
{
    protected private abstract Result Do(
        ILogger logger, string name, Arg arg, Action action, Result result);

    private Func<Result> Doing(ILogger logger, string name, Action action)
        => Invoking(() => Do(logger, name, arg: new(), action, result: new()));

    internal virtual DoAssertions Assertions
        => ActivityDoAssertions.Instance;

    [Test]
    public void Do_NullName()
    {
        var logger = new FakeLogger();

        static void Op()
            => Assert.Fail("This delegate should not be invoked.");

        Doing(logger, name: null!, Op)
            .Should().Throw<ArgumentNullException>().WithParameterName("name");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public void Do_EmptyName()
    {
        var logger = new FakeLogger();

        static void Op()
            => Assert.Fail("This delegate should not be invoked.");

        Doing(logger, name: "", Op)
            .Should().Throw<ArgumentException>().WithParameterName("name");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public void Do_NullAction()
    {
        var logger = new FakeLogger();

        Doing(logger, name: "a", null!)
            .Should().Throw<ArgumentNullException>().WithParameterName("action");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public void Do_Normal()
    {
        var logger   = new FakeLogger();
        var arg      = new Arg();
        var expected = new Result();

        void Op()
        {
            Assertions.AssertDoStarted(logger, "a");
        }

        Assertions.AssertDoNotStarted(logger);

        var result = Do(logger, name: "a", arg, Op, expected);

        Assertions.AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void Do_Exception()
    {
        var logger = new FakeLogger();

        static void Op()
            => throw new Exception();

        Assertions.AssertDoNotStarted(logger);

        Doing(logger, name: "a", Op)
            .Should().ThrowExactly<Exception>()
            .Which.AssignTo(out var exception);

        Assertions.AssertDoCompleted(logger, "a", exception);
    }
}
