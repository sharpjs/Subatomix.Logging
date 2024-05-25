// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Internal;

[TestFixture]
public class NullScopeTests
{
    [Test]
    public void Dispose_Multiple()
    {
        var disposable = NullScope.Instance;

        disposable.Dispose();
        disposable.Dispose();
    }
}
