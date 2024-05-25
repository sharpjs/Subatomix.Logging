// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using NUnit.Framework.Internal;

namespace Subatomix.Logging.Debugger;

[TestFixture]
public class DebuggerLoggerProviderTests
{
    [Test]
    public void ScopeProvider_Get()
    {
        using var provider = new DebuggerLoggerProvider();

        provider.ScopeProvider.Should().BeNull();
    }

    [Test]
    public void ScopeProvider_Set()
    {
        using var provider = new DebuggerLoggerProvider();

        var scopes = Mock.Of<IExternalScopeProvider>();

        provider.SetScopeProvider(scopes);

        provider.ScopeProvider.Should().BeSameAs(scopes);
    }

    [Test]
    public void CreateLogger_NullCategoryName()
    {
        using var provider = new DebuggerLoggerProvider();

        provider
            .Invoking(p => p.CreateLogger(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CreateLogger_Normal()
    {
        using var provider = new DebuggerLoggerProvider();

        var name = Any.GetString();

        provider
            .CreateLogger(name)
            .Should().BeOfType<DebuggerLogger>()
            .Which.Name.Should().BeSameAs(name);
    }
}
