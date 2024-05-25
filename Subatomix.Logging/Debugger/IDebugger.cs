// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Debugger;

internal interface IDebugger
{
    bool IsAttached { get; }

    void Log(int level, string category, string message);
}
