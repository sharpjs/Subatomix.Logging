// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Fake;
using Subatomix.Logging.Testing;

namespace Subatomix.Logging;

[TestFixture]
public class ActivityScopeInitiatorTests
{
    [Test]
    public void Begin()
    {
        var logger   = new FakeLogger();
        var logLevel = Any.LogLevelExceptNone();
        var name     = Any.GetString();

        var initiator = new ActivityScopeInitiator(logger, logLevel, name);

        using var scope = initiator.Begin();

        scope            .Should().NotBeNull();
        scope.IsCompleted.Should().BeFalse();
        scope.Logger     .Should().BeSameAs(logger);
        scope.LogLevel   .Should().Be(logLevel);
        scope.Name       .Should().BeSameAs(name);
    }

    [TestFixture]
    public class DoOpTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            logger.Activity(name).Do(action);
            return result;
        }
    }

    [TestFixture]
    public class DoOpWithArgTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            logger.Activity(name).Do(arg, action.Expecting(arg));
            return result;
        }
    }

    [TestFixture]
    public class DoOpWithResultTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            return logger.Activity(name).Do(action.Returning(result));
        }
    }

    [TestFixture]
    public class DoOpWithArgAndResultTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            return logger.Activity(name).Do(arg, action.Expecting(arg).Returning(result));
        }
    }

    [TestFixture]
    public class DoAsyncOpTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            await logger.Activity(name).DoAsync(action);
            return result;
        }
    }

    [TestFixture]
    public class DoAsyncOpWithArgTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            await logger.Activity(name).DoAsync(arg, action.Expecting(arg));
            return result;
        }
    }

    [TestFixture]
    public class DoAsyncOpWithResultTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            return await logger.Activity(name).DoAsync(action.Returning(result));
        }
    }

    [TestFixture]
    public class DoAsyncOpWithArgAndResultTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            return await logger.Activity(name).DoAsync(arg, action.Expecting(arg).Returning(result));
        }
    }
}
