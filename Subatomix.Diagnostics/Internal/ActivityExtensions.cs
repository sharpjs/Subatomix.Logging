using System.Diagnostics;

namespace Subatomix.Diagnostics;

using static ActivityIdFormat;

internal static class ActivityExtensions
{
    internal static void GetRootOperationId(this Activity activity, Span<char> chars)
    {
        var id    = activity.GetRootOperationId();
        var index = 0;

        foreach (char c in id)
        {
            if (index >= chars.Length)
                break;

            if (!char.IsLetterOrDigit(c))
                continue;

            chars[index++] = c;
        }

        chars.Slice(index).Fill('.');
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
