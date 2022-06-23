/*
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

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
    bool Equals(object other);

    /// <inheritdoc cref="Object.GetHashCode" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetHashCode();

    /// <inheritdoc cref="Object.GetType" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type GetType();

    /// <inheritdoc cref="Object.ToString" />
    [EditorBrowsable(EditorBrowsableState.Never)]
    string ToString();
}
