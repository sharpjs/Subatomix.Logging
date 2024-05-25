// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

internal class TestClock : IClock
{
    public DateTime Now { get; set; }
}
