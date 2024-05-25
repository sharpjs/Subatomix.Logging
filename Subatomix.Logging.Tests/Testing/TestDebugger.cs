// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Debugger;

namespace Subatomix.Logging.Testing;

internal class TestDebugger : IDebugger
{
    private readonly List<(int Level, string Category, string Message)>
        _entries = new();

    public IReadOnlyList<(int Level, string Category, string Message)>
        Entries => _entries;

    public bool IsAttached { get; set; }

    public void Log(int level, string category, string message)
        => _entries.Add((level, category, message));
}
