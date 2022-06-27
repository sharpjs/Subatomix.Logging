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

using Subatomix.Logging.Testing;

namespace Subatomix.Logging;

using static LogLevel;

[TestFixture]
public class OperationScopeInitiatorTests
{
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

    [TestFixture]
    public class DoOpTests : DoTests
    {
        internal override DoAssertions Assertions
            => DoAssertions.Instance;

        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            logger.Operation(name).Do(action);
            return result;
        }
    }

    [TestFixture]
    public class DoOpWithArgTests : DoTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            logger.Operation(name).Do(arg, action.Expecting(arg));
            return result;
        }
    }

    [TestFixture]
    public class DoOpWithResultTests : DoTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            return logger.Operation(name).Do(action.Returning(result));
        }
    }

    [TestFixture]
    public class DoOpWithArgAndResultTests : DoTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            return logger.Operation(name).Do(arg, action.Expecting(arg).Returning(result));
        }
    }

    [TestFixture]
    public class DoAsyncOpTests : DoAsyncTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            await logger.Operation(name).DoAsync(action);
            return result;
        }
    }

    [TestFixture]
    public class DoAsyncOpWithArgTests : DoAsyncTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            await logger.Operation(name).DoAsync(arg, action.Expecting(arg));
            return result;
        }
    }

    [TestFixture]
    public class DoAsyncOpWithResultTests : DoAsyncTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            return await logger.Operation(name).DoAsync(action.Returning(result));
        }
    }

    [TestFixture]
    public class DoAsyncOpWithArgAndResultTests : DoAsyncTests
    {
        internal override DoAssertions Assertions => DoAssertions.Instance;

        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            return await logger.Operation(name).DoAsync(arg, action.Expecting(arg).Returning(result));
        }
    }
}
