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

namespace Subatomix.Logging.Debugger;

/// <summary>
///   A provider of <see cref="DebuggerLogger"/> instances.
/// </summary>
[ProviderAlias("Debugger")]
public class DebuggerLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    /// <summary>
    ///   Gets the external scope provider, if any.  Defaults to
    ///   <see langword="null"/>.
    /// </summary>
    public IExternalScopeProvider? ScopeProvider { get; private set; }

    /// <inheritdoc/>
    public void SetScopeProvider(IExternalScopeProvider? scopeProvider)
    {
        ScopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new DebuggerLogger(this, categoryName);
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
