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

using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics.Logging.Debug;

[TestFixture, Explicit("To be run interactively for ad-hoc testing.")]
public class IntegrationTests
{
    [Test]
    public void Run()
    {
        using var factory = LoggerFactory.Create(f => f
            .SetMinimumLevel(LogLevel.Trace)
            .ClearProviders()
            .AddDebug()
        );

        factory
            .CreateLogger<IntegrationTests>()
            .LogInformation(
                "You should see {what} in the {where}.",
                "this",
                "Debug Output window"
            );

        System.Diagnostics.Debugger.Break();
    }
}
