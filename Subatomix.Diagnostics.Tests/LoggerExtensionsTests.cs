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
using Subatomix.Diagnostics.Testing;

namespace Subatomix.Diagnostics;

using static ExceptionTestHelpers;
using static LogLevel;

[TestFixture]
public partial class LoggerExtensionsTests
{
    [Test]
    public void Trace_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Trace("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Trace, "abc", null as Exception));
    }

    [Test]
    public void Trace_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Trace(exception);

        logger.Entries.Should().Equal((Trace, "", exception));
    }

    [Test]
    public void Debug_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Debug("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Debug, "abc", null as Exception));
    }

    [Test]
    public void Debug_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Debug(exception);

        logger.Entries.Should().Equal((Debug, "", exception));
    }

    [Test]
    public void Info_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Info("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Information, "abc", null as Exception));
    }

    [Test]
    public void Info_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Info(exception);

        logger.Entries.Should().Equal((Information, "", exception));
    }

    [Test]
    public void Warn_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Warn("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Warning, "abc", null as Exception));
    }

    [Test]
    public void Warn_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Warn(exception);

        logger.Entries.Should().Equal((Warning, "", exception));
    }

    [Test]
    public void Error_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Error("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Error, "abc", null as Exception));
    }

    [Test]
    public void Error_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Error(exception);

        logger.Entries.Should().Equal((Error, "", exception));
    }

    [Test]
    public void Critical_StringAndArgs()
    {
        var logger = new TestLogger();

        logger.Critical("a{foo}{bar}", "b", "c");

        logger.Entries.Should().Equal((Critical, "abc", null as Exception));
    }

    [Test]
    public void Critical_Exception()
    {
        var logger    = new TestLogger();
        var exception = Thrown("a");

        logger.Critical(exception);

        logger.Entries.Should().Equal((Critical, "", exception));
    }

    [Test]
    public void BeginOperation_Name()
    {
        var logger = new TestLogger();

        // Relying on [CallerMemberName] for name
        using (var scope = logger.BeginOperation())
        {
            scope         .Should().NotBeNull();
            scope.Name    .Should().Be(nameof(BeginOperation_Name));
            scope.Logger  .Should().BeSameAs(logger);
            scope.LogLevel.Should().Be(Information);
        }
    }

    [Test]
    public void BeginOperation_LevelAndName()
    {
        var logger = new TestLogger();

        // Relying on [CallerMemberName] for name
        using (var scope = logger.BeginOperation(Debug))
        {
            scope         .Should().NotBeNull();
            scope.Name    .Should().Be(nameof(BeginOperation_LevelAndName));
            scope.Logger  .Should().BeSameAs(logger);
            scope.LogLevel.Should().Be(Debug);
        }
    }

    [Test]
    public void BeginActivity_Name()
    {
        var logger = new TestLogger();

        // Relying on [CallerMemberName] for name
        using (var scope = logger.BeginActivity())
        {
            scope         .Should().NotBeNull();
            scope.Name    .Should().Be(nameof(BeginActivity_Name));
            scope.Logger  .Should().BeSameAs(logger);
            scope.LogLevel.Should().Be(Information);
        }
    }

    [Test]
    public void BeginActivity_LevelAndName()
    {
        var logger = new TestLogger();

        // Relying on [CallerMemberName] for name
        using (var scope = logger.BeginActivity(Debug))
        {
            scope         .Should().NotBeNull();
            scope.Name    .Should().Be(nameof(BeginActivity_LevelAndName));
            scope.Logger  .Should().BeSameAs(logger);
            scope.LogLevel.Should().Be(Debug);
        }
    }
}
