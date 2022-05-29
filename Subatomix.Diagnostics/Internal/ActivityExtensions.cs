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

namespace Subatomix.Diagnostics;

using static ActivityIdFormat;

internal static class ActivityExtensions
{
    internal static void GetRootOperationId(this Activity activity, Span<char> chars)
    {
        activity
            .GetRootOperationId()
            .FillLettersAndDigits(chars);
    }

    internal static string GetRootOperationId(this Activity activity)
    {
        if (activity is null)
            throw new ArgumentNullException(nameof(activity));

        return activity.IdFormat == W3C
            ? activity.GetRootOperationIdW3C()
            : activity.GetRootOperationIdFallback();
    }

    private static string GetRootOperationIdW3C(this Activity activity)
    {
        var id = activity.TraceId;
        if (id == default)
            throw MakeNotStartedException();

        return id.ToHexString();
    }

    private static string GetRootOperationIdFallback(this Activity activity)
    {
        var id = activity.RootId;
        if (id is null)
            throw MakeNotStartedException();

        return id;
    }

    private static Exception MakeNotStartedException()
    {
        return new InvalidOperationException(
            "Cannot get root operation id for an unstarted Action."
        );
    }
}
