// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

/// <summary>
///   Provides the current UTC date and time.
/// </summary>
internal sealed class UtcClock : IClock
{
    private UtcClock() { }

    /// <summary>
    ///   Gets the singleton instance of <see cref="UtcClock"/>.
    /// </summary>
    public static UtcClock Instance { get; } = new UtcClock();

    /// <inheritdoc/>
    public DateTime Now => DateTime.UtcNow;
}
