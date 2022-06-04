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

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Subatomix.Diagnostics.Testing;

namespace Subatomix.Diagnostics;

using static ExceptionTestHelpers;

[TestFixture]
public class CorrelationScopeTests
{
    [Test]
    public void Construct_NullLogger()
    {
        Invoking(() => new OperationScope(null!, default, "a"))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Test]
    public void Construct_NullName()
    {
        Invoking(() => new OperationScope(NullLogger.Instance, default, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void Construct_EmptyName()
    {
        Invoking(() => new OperationScope(NullLogger.Instance, default, ""))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Activity_Get()
    {
        using var s = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a");

        s.Activity.Should().BeSameAs(Activity.Current);
    }

    [Test]
    [NonParallelizable] // becuase it depends on static state
    public void CreateAndDispose_W3C()
    {
        using var _ = new ActivityTestScope(ActivityIdFormat.W3C);
        using var s = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a");

        var a = Activity.Current!;
        a               .Should().NotBeNull ();
        a.OperationName .Should().Be        ("a");
        a.Parent        .Should().BeNull    ();
        a.IdFormat      .Should().Be        (ActivityIdFormat.W3C);
        // W3C
        a.TraceId       .Should().NotBe     (default(ActivityTraceId));
        a.ParentSpanId  .Should().Be        (default(ActivitySpanId));
        a.SpanId        .Should().NotBe     (default(ActivitySpanId));
        // Hierarchical
        a.RootId        .Should().Be        (a.TraceId.ToHexString());
        a.ParentId      .Should().BeNull    ();
        // Composite
        a.Id            .Should().Be        ($"00-{a.TraceId}-{a.SpanId}-00");

        using (new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "b"))
        {
            var b = Activity.Current!;
            b               .Should().NotBeNull ();
            b.OperationName .Should().Be        ("b");
            b.Parent        .Should().BeSameAs  (a);
            b.IdFormat      .Should().Be        (ActivityIdFormat.W3C);
            // W3C
            b.TraceId       .Should().Be        (a.TraceId);
            b.ParentSpanId  .Should().Be        (a.SpanId);
            b.SpanId        .Should().NotBe     (a.SpanId);
            b.SpanId        .Should().NotBe     (default(ActivitySpanId));
            // Hierarchical
            b.RootId        .Should().Be        (a.TraceId.ToHexString());
            b.ParentId      .Should().Be        (a.Id);
            // Composite
            b.Id            .Should().Be        ($"00-{a.TraceId}-{b.SpanId}-00");
        }

        Activity.Current.Should().BeSameAs(a);

        ((IDisposable) s).Dispose();

        Activity.Current.Should().BeNull();
    }

    [Test]
    [NonParallelizable] // becuase it depends on static state
    public void CreateAndDispose_Hierarchical()
    {
        using var _ = new ActivityTestScope(ActivityIdFormat.Hierarchical);
        using var s = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a");

        var a = Activity.Current!;
        a               .Should().NotBeNull     ();
        a.OperationName .Should().Be            ("a");
        a.Parent        .Should().BeNull        ();
        a.IdFormat      .Should().Be            (ActivityIdFormat.Hierarchical);
        // W3C
        a.TraceId       .Should().Be            (default(ActivityTraceId));
        a.ParentSpanId  .Should().Be            (default(ActivitySpanId));
        a.SpanId        .Should().Be            (default(ActivitySpanId));
        // Hierarchical
        a.RootId        .Should().MatchRegex    (@"^[0-9a-f]{1,8}-[0-9a-f]{1,16}$");
        a.RootId        .Should().NotMatchRegex (@"^0{1,8}-0{1,16}$");
        a.ParentId      .Should().BeNull        ();
        // Composite
        a.Id            .Should().Be            ($"|{a.RootId}.");

        using (new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "b"))
        {
            var b = Activity.Current!;
            b               .Should().NotBeNull ();
            b.OperationName .Should().Be        ("b");
            b.Parent        .Should().BeSameAs  (a);
            b.IdFormat      .Should().Be        (ActivityIdFormat.Hierarchical);
            // W3C
            b.TraceId       .Should().Be        (default(ActivityTraceId));
            b.ParentSpanId  .Should().Be        (default(ActivitySpanId));
            b.SpanId        .Should().Be        (default(ActivitySpanId));
            // Hierarchical
            b.RootId        .Should().Be        (a.RootId);
            b.ParentId      .Should().Be        (a.Id);
            // Composite
            b.Id            .Should().Be        ($"|{a.RootId}.1.");
        }

        Activity.Current.Should().BeSameAs(a);

        ((IDisposable) s).Dispose();

        Activity.Current.Should().BeNull();
    }

    [Test]
    public void Dispose_ActivityStatus_ImplicitOk()
    {
        Activity activity;

        using (var scope = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
        }

        activity.Status           .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription.Should().BeNull();
    }

    [Test]
    public void Dispose_ActivityStatus_ImplicitError()
    {
        Activity activity;

        using (var scope = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
            scope.Exception = Thrown();
        }

        activity.Status           .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("A test exception was thrown.");
    }

    [Test]
    public void Dispose_ActivityStatus_ExplicitError()
    {
        Activity activity;

        using (var scope = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;

            // Explicit status takes precedence
            activity.SetStatus(ActivityStatusCode.Error, "Uh-oh.");
        }

        activity.Status           .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Uh-oh.");
    }

    [Test]
    public void Dispose_ActivityStatus_ExplicitOk()
    {
        Activity activity;

        using (var scope = new CorrelationScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
            scope.Exception = Thrown();

            // Explicit status takes precedence
            activity.SetStatus(ActivityStatusCode.Ok, null);
        }

        activity.Status           .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription.Should().BeNull();
    }
}
