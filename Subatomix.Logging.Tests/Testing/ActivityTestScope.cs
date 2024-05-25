// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Testing;

internal class ActivityTestScope : IDisposable
{
    private readonly ActivityIdFormat _priorFormat;
    private readonly bool             _priorForced;

    public ActivityTestScope()
    {
        _priorFormat = Activity.DefaultIdFormat;
        _priorForced = Activity.ForceDefaultIdFormat;

        ShouldNotHaveCurrentActivity();
    }

    public ActivityTestScope(ActivityIdFormat format, bool forced = true)
        : this()
    {
        Activity.DefaultIdFormat      = format;
        Activity.ForceDefaultIdFormat = forced;
    }

    void IDisposable.Dispose()
    {
        Activity.DefaultIdFormat      = _priorFormat;
        Activity.ForceDefaultIdFormat = _priorForced;

        ShouldNotHaveCurrentActivity();
    }

    public static void ShouldNotHaveCurrentActivity()
    {
        Activity.Current.Should().BeNull();
    }
}
