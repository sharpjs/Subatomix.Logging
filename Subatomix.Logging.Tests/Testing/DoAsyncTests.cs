// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;

namespace Subatomix.Logging.Testing;

public abstract class DoAsyncTests
{
    protected private abstract Task<Result> DoAsync(
        ILogger logger, string name, Arg arg, Func<Task> action, Result result);

    private Func<Task<Result>> DoingAsync(ILogger logger, string name, Func<Task> action)
        => Awaiting(() => DoAsync(logger, name, arg: new(), action, result: new()));

    internal virtual DoAssertions Assertions
        => ActivityDoAssertions.Instance;

    [Test]
    public async Task DoAsync_NullName()
    {
        var logger = new FakeLogger();

        static Task Op()
            => throw new AssertionException("This delegate should not be invoked.");

        await DoingAsync(logger, name: null!, Op)
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("name");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public async Task DoAsync_EmptyName()
    {
        var logger = new FakeLogger();

        static Task Op()
            => throw new AssertionException("This delegate should not be invoked.");

        await DoingAsync(logger, name: "", Op)
            .Should().ThrowAsync<ArgumentException>().WithParameterName("name");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public async Task DoAsync_NullAction()
    {
        var logger = new FakeLogger();

        await DoingAsync(logger, name: "a", null!)
            .Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");

        Assertions.AssertDoNotStarted(logger);
    }

    [Test]
    public async Task DoAsync_Normal()
    {
        var logger   = new FakeLogger();
        var arg      = new Arg();
        var expected = new Result();

        Task Op()
        {
            Assertions.AssertDoStarted(logger, "a");
            return Task.CompletedTask;
        }

        Assertions.AssertDoNotStarted(logger);

        var result = await DoAsync(logger, name: "a", arg, Op, expected);

        Assertions.AssertDoCompleted(logger, "a");

        result.Should().BeSameAs(expected);
    }

    [Test]
    public async Task DoAsync_Exception()
    {
        var logger = new FakeLogger();

        static Task Op()
            => throw new Exception();

        Assertions.AssertDoNotStarted(logger);

        (await DoingAsync(logger, name: "a", Op)
            .Should().ThrowExactlyAsync<Exception>()
        )   .Which.AssignTo(out var exception);

        Assertions.AssertDoCompleted(logger, "a", exception);
    }
}
