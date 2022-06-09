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
public class ActivityScopeTests
{
    [Test]
    public void Construct_NullLogger()
    {
        Invoking(() => new ActivityScope(null!, default, "a"))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Test]
    public void Construct_NullName()
    {
        Invoking(() => new ActivityScope(NullLogger.Instance, default, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Test]
    public void Construct_EmptyName()
    {
        Invoking(() => new ActivityScope(NullLogger.Instance, default, ""))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("name");
    }

    [Test]
    public void Activity_Get()
    {
        using var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a");

        scope.Activity.Should().BeSameAs(Activity.Current);
    }

    [Test]
    [NonParallelizable] // becuase it depends on static state
    public void CreateAndDispose_W3C()
    {
        using var _      = new ActivityTestScope(ActivityIdFormat.W3C);
        using var scope0 = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a");

        var root = Activity.Current!;
        root               .Should().NotBeNull ();
        root               .Should().BeSameAs  (scope0.Activity);
        root.OperationName .Should().Be        ("a");
        root.Parent        .Should().BeNull    ();
        root.IdFormat      .Should().Be        (ActivityIdFormat.W3C);
        // W3C
        root.TraceId       .Should().NotBe     (default(ActivityTraceId));
        root.ParentSpanId  .Should().Be        (default(ActivitySpanId));
        root.SpanId        .Should().NotBe     (default(ActivitySpanId));
        // Hierarchical
        root.RootId        .Should().Be        (root.TraceId.ToHexString());
        root.ParentId      .Should().BeNull    ();
        // Composite
        root.Id            .Should().Be        ($"00-{root.TraceId}-{root.SpanId}-00");

        using (var scope1 = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "b"))
        {
            var child = Activity.Current!;
            child               .Should().NotBeNull   ();
            child               .Should().BeSameAs    (scope1.Activity);
            child               .Should().NotBeSameAs (root);
            child.OperationName .Should().Be          ("b");
            child.Parent        .Should().BeSameAs    (root);
            child.IdFormat      .Should().Be          (ActivityIdFormat.W3C);
            // W3C
            child.TraceId       .Should().Be          (root.TraceId);
            child.ParentSpanId  .Should().Be          (root.SpanId);
            child.SpanId        .Should().NotBe       (root.SpanId);
            child.SpanId        .Should().NotBe       (default(ActivitySpanId));
            // Hierarchical
            child.RootId        .Should().Be          (root.TraceId.ToHexString());
            child.ParentId      .Should().Be          (root.Id);
            // Composite
            child.Id            .Should().Be          ($"00-{root.TraceId}-{child.SpanId}-00");
        }

        Activity.Current.Should().BeSameAs(root);

        ((IDisposable) scope0).Dispose(); // To test multiple disposals

        Activity.Current.Should().BeNull();
    }

    [Test]
    [NonParallelizable] // becuase it depends on static state
    public void CreateAndDispose_Hierarchical()
    {
        using var _      = new ActivityTestScope(ActivityIdFormat.Hierarchical);
        using var scope0 = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a");

        var root = Activity.Current!;
        root               .Should().NotBeNull     ();
        root               .Should().BeSameAs      (scope0.Activity);
        root.OperationName .Should().Be            ("a");
        root.Parent        .Should().BeNull        ();
        root.IdFormat      .Should().Be            (ActivityIdFormat.Hierarchical);
        // W3C
        root.TraceId       .Should().Be            (default(ActivityTraceId));
        root.ParentSpanId  .Should().Be            (default(ActivitySpanId));
        root.SpanId        .Should().Be            (default(ActivitySpanId));
        // Hierarchical
        root.RootId        .Should().MatchRegex    (@"^[0-9a-f]{1,8}-[0-9a-f]{1,16}$");
        root.RootId        .Should().NotMatchRegex (@"^0{1,8}-0{1,16}$");
        root.ParentId      .Should().BeNull        ();
        // Composite
        root.Id            .Should().Be            ($"|{root.RootId}.");

        using (var scope1 = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "b"))
        {
            var child = Activity.Current!;
            child               .Should().NotBeNull   ();
            child               .Should().BeSameAs    (scope1.Activity);
            child               .Should().NotBeSameAs (root);
            child.OperationName .Should().Be          ("b");
            child.Parent        .Should().BeSameAs    (root);
            child.IdFormat      .Should().Be          (ActivityIdFormat.Hierarchical);
            // W3C
            child.TraceId       .Should().Be          (default(ActivityTraceId));
            child.ParentSpanId  .Should().Be          (default(ActivitySpanId));
            child.SpanId        .Should().Be          (default(ActivitySpanId));
            // Hierarchical
            child.RootId        .Should().Be          (root.RootId);
            child.ParentId      .Should().Be          (root.Id);
            // Composite
            child.Id            .Should().Be          ($"|{root.RootId}.1.");
        }

        Activity.Current.Should().BeSameAs(root);

        ((IDisposable) scope0).Dispose(); // To test multiple disposals

        Activity.Current.Should().BeNull();
    }

    [Test]
    public void Dispose_ActivityTags()
    {
        Activity activity;

        using (var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
        }

        activity.GetTagItem("peer.service") .Should().Be("InProc");
        activity.GetTagItem("peer.hostname").Should().Be("internal");
        activity.GetTagItem("db.statement") .Should().Be("Activity: a");
    }

    [Test]
    public void Dispose_ActivityStatus_ImplicitOk()
    {
        Activity activity;

        using (var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
        }

        activity.Status             .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription  .Should().BeNull();
        activity.GetTagItem("error").Should().Be("false");
    }

    [Test]
    public void Dispose_ActivityStatus_ImplicitError()
    {
        Activity activity;

        using (var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
            scope.Exception = Thrown();
        }

        activity.Status             .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription  .Should().Be("A test exception was thrown.");
        activity.GetTagItem("error").Should().Be("true");
    }

    [Test]
    public void Dispose_ActivityStatus_ExplicitError()
    {
        Activity activity;

        using (var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;

            // Explicit status takes precedence
            activity.SetStatus(ActivityStatusCode.Error, "Uh-oh.");
        }

        activity.Status             .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription  .Should().Be("Uh-oh.");
        activity.GetTagItem("error").Should().Be("true");
    }

    [Test]
    public void Dispose_ActivityStatus_ExplicitOk()
    {
        Activity activity;

        using (var scope = new ActivityScope(NullLogger.Instance, LogLevel.Debug, "a"))
        {
            activity = scope.Activity;
            scope.Exception = Thrown();

            // Explicit status takes precedence
            activity.SetStatus(ActivityStatusCode.Ok, null);
        }

        activity.Status             .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription  .Should().BeNull();
        activity.GetTagItem("error").Should().Be("false");
    }
}
