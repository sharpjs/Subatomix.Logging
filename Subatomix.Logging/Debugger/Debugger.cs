// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics.CodeAnalysis;
using Base = System.Diagnostics.Debugger;

namespace Subatomix.Logging.Debugger;

[ExcludeFromCodeCoverage]
internal sealed class Debugger : IDebugger
{
    public static Debugger Instance = new();

    private Debugger() { }

    public bool IsAttached
        => Base.IsAttached;

    public void Log(int level, string category, string message)
        => Base.Log(level, category, message);
}
