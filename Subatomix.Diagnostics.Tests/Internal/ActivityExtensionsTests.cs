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

namespace Subatomix.Diagnostics;

using static ActivityIdFormat;
using static ExceptionTestHelpers;

[TestFixture]
public class ActivityExtensionsTests
{
    [Test]
    public void SetStatusIfUnset_Null()
    {
        Invoking(() => default(Activity)!.SetStatusIfUnset(new()))
            .Should().Throw<ArgumentNullException>().WithParameterName("activity");
    }

    [Test]
    public void SetStatusIfUnset_UnsetOk()
    {
        using var activity = new Activity("a");

        activity.SetStatusIfUnset(null);

        activity.Status           .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription.Should().BeNull();
    }

    [Test]
    public void SetStatusIfUnset_UnsetError()
    {
        using var activity = new Activity("a");

        activity.SetStatusIfUnset(Thrown());

        activity.Status           .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("A test exception was thrown.");
    }

    [Test]
    public void SetStatusIfUnset_AlreadySetError()
    {
        using var activity = new Activity("a");

        activity.SetStatus(ActivityStatusCode.Error, "Uh-oh.");
        activity.SetStatusIfUnset(null); // no effect

        activity.Status           .Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Uh-oh.");
    }

    [Test]
    public void SetStatusIfUnset_AlreadySetOk()
    {
        using var activity = new Activity("a");

        activity.SetStatus(ActivityStatusCode.Ok);
        activity.SetStatusIfUnset(Thrown()); // no effect

        activity.Status           .Should().Be(ActivityStatusCode.Ok);
        activity.StatusDescription.Should().BeNull();
    }

    [Test]
    public void GetRootOperationId_IntoSpan_Null()
    {
        Invoking(() =>
        {
            Span<char> chars = stackalloc char[4];
            default(Activity)!.GetRootOperationId(chars);
        })
        .Should().Throw<ArgumentNullException>().WithParameterName("activity");
    }

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
    public void GetRootOperationId_Null()
    {
        Invoking(() => default(Activity)!.GetRootOperationId())
            .Should().Throw<ArgumentNullException>().WithParameterName("activity");
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

    [Test]
    public void GetRootOperationGuid_Null()
    {
        Invoking(() => default(Activity)!.GetRootOperationGuid())
            .Should().Throw<ArgumentNullException>().WithParameterName("activity");
    }

    [Test]
    public void GetRootOperationGuid_W3C_Started()
    {
        using var activity = new Activity("a")
            .SetIdFormat(W3C)
            .Start();

        // Root operation GUID for W3C format
        var expected = Guid.ParseExact(activity.TraceId.ToHexString(), "N");

        activity.GetRootOperationGuid().Should().Be(expected).And.NotBeEmpty();
    }

    [Test]
    public void GetRootOperationGuid_W3C_NotStarted()
    {
        using var activity = new Activity("a")
            .SetIdFormat(W3C);

        activity
            .Invoking(a => a.GetRootOperationGuid())
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void GetRootOperationGuid_Hierarchical_Started()
    {
        using var activity = new Activity("a")
            .SetIdFormat(Hierarchical)
            .Start();

        // Root operation GUID for hierarchical format
        var expected = ActivityExtensions.SynthesizeGuid(activity.RootId!);

        activity.GetRootOperationGuid().Should().Be(expected).And.NotBeEmpty();
    }

    [Test]
    public void GetRootOperationGuid_Hierarchical_NotStarted()
    {
        using var activity = new Activity("a")
            .SetIdFormat(Hierarchical);

        activity.Invoking(a => a.GetRootOperationGuid())
            .Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCase("",            "62b35a7d-1e8a-44d9-8570-cfbde123804e")]
    [TestCase("a",           "3159ad3f-8f45-486c-83b8-e7def0110412")]
    [TestCase("abcd",        "a62b35a8-91e8-404d-84f7-dc1b3e021ca4")]
    [TestCase("abcdefghijk", "d14c566c-9b23-4258-a8b9-377c04c87823")]
    public void SynthesizeGuid(string source, string guid)
    {
        var expected = Guid.ParseExact(guid, "D");

        ActivityExtensions.SynthesizeGuid(source).Should().Be(expected);
    }
}
