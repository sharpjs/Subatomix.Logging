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

using FluentAssertions.Extensions;
using Subatomix.Logging.Testing;

namespace Sharp.Diagnostics.Logging;

[TestFixture]
[NonParallelizable] // because Log uses a static ILogger
public class TraceOperationTests
{
    [Test]
    public void StartTime_Get()
    {
        using var operation = new TraceOperation();

        operation.StartTime.Should().Be(Activity.Current!.StartTimeUtc);
    }

    [Test]
    public void Duration_Get()
    {
        using var operation = new TraceOperation();

        Thread.Sleep(10.Milliseconds());

        var duration    = operation.Duration;    // base property
        var elapsedTime = operation.ElapsedTime; // compatibility synonym

        duration   .Should().BeGreaterThan(TimeSpan.Zero);
        elapsedTime.Should().BeGreaterThan(TimeSpan.Zero);
        elapsedTime.Should().BeCloseTo(duration, precision: 1.Milliseconds());
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoOpTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            Log.Logger = logger;
            TraceOperation.Do(name, action);
            return result;
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoTraceSourceAndOpTests : DoTests
    {
        protected private override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            Log.Logger = logger;
            TraceOperation.Do(null, name, action);
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
            return TraceOperation.Do(name, action.Returning(result));
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoTraceSourceAndOpWithResultTests : DoTests
    {
        private protected override Result Do(
            ILogger logger, string name, Arg arg, Action action, Result result)
        {
            Log.Logger = logger;
            return TraceOperation.Do(null, name, action.Returning(result));
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
            await TraceOperation.DoAsync(name, action);
            return result;
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoAsyncTraceSourceAndOpTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            Log.Logger = logger;
            await TraceOperation.DoAsync(null, name, action);
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
            await TraceOperation.DoAsync(name, action.Returning(result));
            return result;
        }
    }

    [TestFixture]
    [NonParallelizable] // because Log uses a static ILogger
    public class DoAsyncTraceSourceAndOpWithResultTests : DoAsyncTests
    {
        private protected override async Task<Result> DoAsync(
            ILogger logger, string name, Arg arg, Func<Task> action, Result result)
        {
            Log.Logger = logger;
            await TraceOperation.DoAsync(null, name, action.Returning(result));
            return result;
        }
    }
}
