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

using Microsoft.Extensions.Logging.Abstractions;

namespace Subatomix.Logging.Testing;

public abstract class DoTests : DoTestsBase
{
    protected private abstract Result Do(
        ILogger logger, string name, Arg arg, Action action, Result result);

    private Func<Result> Doing(ILogger logger, string name, Action action)
        => Invoking(() => Do(logger, name, arg: new(), action, result: new()));

    [Test]
    public void Do_NullName()
    {
        var logger = new TestLogger();

        static void Op()
            => Assert.Fail("This delegate should not be invoked.");

        Doing(logger, name: null!, Op)
            .Should().Throw<ArgumentNullException>().WithParameterName("name");

        logger.Entries2.Should().BeEmpty();
    }

    [Test]
    public void Do_EmptyName()
    {
        var logger = new TestLogger();

        static void Op()
            => Assert.Fail("This delegate should not be invoked.");

        Doing(logger, name: "", Op)
            .Should().Throw<ArgumentException>().WithParameterName("name");

        logger.Entries2.Should().BeEmpty();
    }

    [Test]
    public void Do_NullAction()
    {
        var logger   = new TestLogger();
        var arg      = new Arg();
        var expected = new Result();

        var op = null as Action;

        Doing(logger, name: "a", op!)
            .Should().Throw<ArgumentNullException>().WithParameterName("action");

        logger.Entries2.Should().BeEmpty();
    }

    [Test]
    public void Do_Normal()
    {
        var logger   = new TestLogger();
        var arg      = new Arg();
        var expected = new Result();

        void Op()
        {
            AssertDoStarted(logger, "a");

            Activity.Current               .Should().NotBeNull();
            Activity.Current!.OperationName.Should().Be("a");
        }

        Activity.Current.Should().BeNull();

        var result = Do(logger, name: "a", arg, Op, expected);

        AssertDoCompleted(logger, "a");

        Activity.Current.Should().BeNull();

        result.Should().BeSameAs(expected);
    }

    [Test]
    public void Do_Exception()
    {
        var logger = new TestLogger();

        static void Op()
            => throw new Exception();

        Activity.Current.Should().BeNull();

        Doing(logger, name: "a", Op)
            .Should().ThrowExactly<Exception>()
            .Which.AssignTo(out var exception);

        AssertDoCompleted(logger, "a", exception);

        Activity.Current.Should().BeNull();
    }
}
