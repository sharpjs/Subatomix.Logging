// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.ComponentModel;

namespace Subatomix.Logging;

/// <summary>
///   Interface for fluent APIs.  Prevents methods inherited from
///   <see cref="System.Object"/> from appearing in IntelliSense.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IFluent
{
    /// <inheritdoc cref="Object.Equals(object)" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool Equals(object? other);

    /// <inheritdoc cref="Object.GetHashCode" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetHashCode();

    /// <inheritdoc cref="Object.GetType" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type GetType();

    /// <inheritdoc cref="Object.ToString" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    string? ToString();
}
