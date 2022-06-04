using System.Diagnostics;
using Subatomix.Diagnostics.Testing;

namespace Subatomix.Diagnostics.Legacy;

using static ActivityTestScope;

[TestFixture]
[NonParallelizable] // becuase it depends on static state
internal class CorrelationManagerActivityListenerTests
{
    [Test]
    public void StartAndStopActivity_W3C()
    {
        using var _        = new ActivityTestScope(ActivityIdFormat.W3C);
        using var listener = new CorrelationManagerActivityListener();
        Guid activityId;

        listener.Register();

        using (var a = new Activity("a").Start())
        {
            activityId = Guid.ParseExact(a.TraceId.ToHexString(), "N");

            LegacyActivityId    .Should().Be(activityId);
            LegacyOperationStack.Should().Equal(new[] { a.Id });

            using (var b = new Activity("b").Start())
            {
                LegacyActivityId    .Should().Be(activityId);
                LegacyOperationStack.Should().Equal(new[] { b.Id, a.Id });
            }

            LegacyActivityId    .Should().Be(activityId);
            LegacyOperationStack.Should().Equal(new[] { a.Id });
        }

        LegacyActivityId    .Should().BeEmpty();
        LegacyOperationStack.Should().BeEmpty();
    }

    [Test]
    public void StartAndStopActivity_Hierarchical()
    {
        using var _        = new ActivityTestScope(ActivityIdFormat.Hierarchical);
        using var listener = new CorrelationManagerActivityListener();
        Guid activityId;

        listener.Register();

        using (var a = new Activity("a").Start())
        {
            LegacyActivityId    .Should().NotBeEmpty();
            LegacyOperationStack.Should().Equal(new[] { a.Id });
            activityId = LegacyActivityId;

            using (var b = new Activity("b").Start())
            {
                LegacyActivityId    .Should().Be(activityId);
                LegacyOperationStack.Should().Equal(new[] { b.Id, a.Id });
            }

            LegacyActivityId    .Should().Be(activityId);
            LegacyOperationStack.Should().Equal(new[] { a.Id });
        }

        LegacyActivityId    .Should().BeEmpty();
        LegacyOperationStack.Should().BeEmpty();
    }
}
