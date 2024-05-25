// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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
