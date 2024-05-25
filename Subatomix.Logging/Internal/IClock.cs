// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

/// <summary>
///   Provides the current date and time.
/// </summary>
internal interface IClock
{
    /// <summary>
    ///   Gets the current date and time.
    /// </summary>
    DateTime Now { get; }
}
