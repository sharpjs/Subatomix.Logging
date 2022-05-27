using System.Diagnostics;

namespace Subatomix.Diagnostics;

using static ActivityIdFormat;

[TestFixture]
public class ActivityExtensionsTests
{
    [Test]
    [TestCase(W3C)]
    [TestCase(Hierarchical)]
    public void GetRootOperationId_IntoSpan_W3C(ActivityIdFormat format)
    {
        using var activity = new Activity("a")
            .SetIdFormat(format)
            .Start();

        var expected = activity
            .GetRootOperationId()
            .Where(char.IsLetterOrDigit)
            .Take(4)
            .ToArray();

        Span<char> chars = stackalloc char[4];
        activity.GetRootOperationId(chars);

        chars.ToArray().Should().Equal(expected);
    }

    [Test]
    public void GetRootOperationId_W3C_Started()
    {
        using var activity = new Activity("a")
            .SetIdFormat(W3C)
            .Start();

        // Root operation id for W3C format
        var expected = activity.TraceId.ToHexString();

        activity.GetRootOperationId().Should().Be(expected);
    }

    [Test]
    public void GetRootOperationId_W3C_NotStarted()
    {
        using var activity = new Activity("a")
            .SetIdFormat(W3C);

        activity
            .Invoking(a => a.GetRootOperationId())
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void GetRootOperationId_Hierarchical_Started()
    {
        using var activity = new Activity("a")
            .SetIdFormat(Hierarchical)
            .Start();

        // Root operation id for hierarchical format
        var expected = activity.RootId!;

        activity.GetRootOperationId().Should().Be(expected);
    }

    [Test]
    public void GetRootOperationId_Hierarchical_NotStarted()
    {
        using var activity = new Activity("a")
            .SetIdFormat(Hierarchical);

        activity.Invoking(a => a.GetRootOperationId())
            .Should().Throw<InvalidOperationException>();
    }
}
