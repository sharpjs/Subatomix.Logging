// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Globalization;
using System.Text;

namespace Subatomix.Logging.Legacy;

internal static class Formatters
{
    public static Func<Void, Exception?, string>
        Empty { get; } = Format;

    public static Func<int, Exception?, string>
        MessageId { get; } = Format;

    public static Func<string?, Exception?, string>
        Message { get; } = Format;

    public static Func<(string?, string?), Exception?, string>
        MessageAndDetail { get; } = Format;

    public static Func<(string?, object?[]?), Exception?, string>
        Template { get; } = Format;

    public static Func<(string?, Guid), Exception?, string>
        Transfer { get; } = Format;

    public static Func<object?, Exception?, string>
        Data { get; } = Format;

    public static Func<object?[]?, Exception?, string>
        DataArray { get; } = Format;

    private static string Format(Void state, Exception? _)
    {
        return string.Empty;
    }

    private static string Format(int state, Exception? _)
    {
#if NET6_0_OR_GREATER
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Message ID: {state}"
        );
#else
        return string.Format(
            CultureInfo.InvariantCulture,
            "Message ID: {0}", state
        );
#endif
    }

    private static string Format(string? state, Exception? _)
    {
        return state ?? string.Empty;
    }

    private static string Format((string?, string?) state, Exception? _)
    {
        var (message, detailedMessage) = state;

        if (string.IsNullOrEmpty(message))
            return detailedMessage ?? string.Empty;

        if (string.IsNullOrEmpty(detailedMessage))
            return message!;

        return string.Concat(message!, " ", detailedMessage!);
    }

    private static string Format((string?, object?[]?) state, Exception? _)
    {
        var (template, args) = state;

        return string.Format(
            CultureInfo.InvariantCulture,
            template ?? string.Empty,
            args     ?? Array.Empty<object>()
        );
    }

    private static string Format((string?, Guid) state, Exception? _)
    {
        var (message, activityId) = state;

#if NET6_0_OR_GREATER
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{message} {{related:{activityId}}}"
        );
#else
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} {{related:{1}}}", message, activityId
        );
#endif
    }

    private static string Format(object? state, Exception? _)
    {
        return state?.ToString() ?? string.Empty;
    }

    private static string Format(object?[]? state, Exception? _)
    {
        // Cannot use string.Join due to an unfortunate behavior quirk in it:
        // if array[0] is null, it returns an empty string instead of joining.

        switch (state)
        {
            case null or { Length: 0 }:
                return string.Empty;

            case { Length: 1 }:
                return Format(state[0], _);

            default:
                var s = new StringBuilder();

                s.Append(state[0]);

                for (int i = 1; i < state.Length; i++)
                    s.Append(", ").Append(state[i]);

                return s.ToString();
        }
    }
}
