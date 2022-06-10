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

using System.Diagnostics;

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
