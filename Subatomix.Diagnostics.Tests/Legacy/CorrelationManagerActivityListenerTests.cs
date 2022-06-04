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
using Subatomix.Diagnostics.Testing;

namespace Subatomix.Diagnostics.Legacy;

[TestFixture]
[NonParallelizable] // becuase it depends on static state
internal class CorrelationManagerActivityListenerTests
{
    [Test]
    public void StartAndStopActivity_W3C()
    {
        using var _        = new ActivityTestScope(ActivityIdFormat.W3C);
        using var listener = new CorrelationManagerActivityListener();

        listener.Register();

        AssertCorrelationManagerEmpty();

        using (var a = new Activity("a").Start())
        {
            var activityId = Guid.ParseExact(a.TraceId.ToHexString(), "N");

            AssertCorrelationManager(activityId, a.Id);

            using (var b = new Activity("b").Start())
                AssertCorrelationManager(activityId, b.Id, a.Id);

            AssertCorrelationManager(activityId, a.Id);
        }

        AssertCorrelationManagerEmpty();
    }

    [Test]
    public void StartAndStopActivity_Hierarchical()
    {
        using var _        = new ActivityTestScope(ActivityIdFormat.Hierarchical);
        using var listener = new CorrelationManagerActivityListener();

        listener.Register();

        AssertCorrelationManagerEmpty();

        using (var a = new Activity("a").Start())
        {
            AssertCorrelationManager(out var activityId, a.Id);

            using (var b = new Activity("b").Start())
                AssertCorrelationManager(activityId, b.Id, a.Id);

            AssertCorrelationManager(activityId, a.Id);
        }

        AssertCorrelationManagerEmpty();
    }

    [Test]
    [TestCaseSource(nameof(ActivityIdFormats))]
    public void StartAndStopActivity_Ignored(ActivityIdFormat format)
    {
        using var _        = new ActivityTestScope(format);
        using var listener = new TestListener();
        listener.Register();

        using var a = new Activity("ignore").Start();

        AssertCorrelationManagerEmpty();
    }

    [Test]
    [TestCaseSource(nameof(ActivityIdFormats))]
    public void StartAndStopActivity_ViaSource_Ignored(ActivityIdFormat format)
    {
        using var _        = new ActivityTestScope(format);
        using var source   = new ActivitySource("ignore");
        using var listener = new TestListener();
        listener.Register();

        source.CreateActivity("a", default)
            .Should().BeNull(because: "the listener should ignore this source");
    }

    [Test]
    [TestCaseSource(nameof(ActivityIdFormats))]
    public void StartAndStopActivity_ViaSource_NotIgnored(ActivityIdFormat format)
    {
        using var _        = new ActivityTestScope(format);
        using var source   = new ActivitySource("foo");
        using var listener = new TestListener();
        listener.Register();

        AssertCorrelationManagerEmpty();

        using (var a = source.StartActivity("a")!)
        {
            AssertCorrelationManager(out var activityId, a.Id);

            // This form is required to exercise SampleUsingParentId
            //                                       vvvvvvvvvvvvvv
            using (var b = source.StartActivity("b", default, a.Id!)!)
                AssertCorrelationManager(activityId, b.Id, a.Id);

            AssertCorrelationManager(activityId, a.Id);
        }

        AssertCorrelationManagerEmpty();
    }

    public static IEnumerable<ActivityIdFormat> ActivityIdFormats => new[]
    {
        ActivityIdFormat.W3C,
        ActivityIdFormat.Hierarchical,
    };

    private static Guid LegacyActivityId
        => Trace.CorrelationManager.ActivityId;

    private static IEnumerable<object?> LegacyOperationStack
        => Trace.CorrelationManager.LogicalOperationStack.Cast<object?>();

    private static void AssertCorrelationManagerEmpty()
    {
        LegacyActivityId    .Should().BeEmpty();
        LegacyOperationStack.Should().BeEmpty();
    }

    private static void AssertCorrelationManager(out Guid activityId, params object?[] stack)
    {
        LegacyActivityId    .Should().NotBeEmpty();
        LegacyOperationStack.Should().Equal(stack);
        activityId = LegacyActivityId;
    }

    private static void AssertCorrelationManager(Guid activityId, params object?[] stack)
    {
        LegacyActivityId    .Should().Be(activityId);
        LegacyOperationStack.Should().Equal(stack);
    }

    private class TestListener : CorrelationManagerActivityListener
    {
        protected override bool ShouldFlow(ActivitySource source)
            => source.Name != "ignore";

        protected override bool ShouldFlow(Activity activity)
            => activity.OperationName != "ignore";
    }
}
