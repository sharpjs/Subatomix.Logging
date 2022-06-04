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
