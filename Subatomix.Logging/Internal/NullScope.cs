// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

/// <summary>
///   An empty scope that can be disposed any number of times.
/// </summary>
internal sealed class NullScope : IDisposable
{
    /// <summary>
    ///   The singleton instance of <see cref="NullScope"/>.
    /// </summary>
    public static IDisposable Instance { get; } = new NullScope();

    private NullScope() { }

    /// <inheritdoc/>
    public void Dispose() { }
}
