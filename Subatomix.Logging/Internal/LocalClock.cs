// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

/// <summary>
///   Provides the current local date and time.
/// </summary>
internal sealed class LocalClock : IClock
{
    private LocalClock() { }

    /// <summary>
    ///   Gets the singleton instance of <see cref="LocalClock"/>.
    /// </summary>
    public static LocalClock Instance { get; } = new LocalClock();

    /// <inheritdoc/>
    public DateTime Now => DateTime.Now;
}
