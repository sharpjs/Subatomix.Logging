// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Testing;

internal static class Extensions
{
    // Rightward assignment
    internal static T AssignTo<T>(this T value, out T location)
        => location = value;
}
