// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Debugger;

[TestFixture, Explicit("To be run interactively for ad-hoc testing.")]
public class IntegrationTests
{
    [Test]
    public void Run()
    {
        using var factory = LoggerFactory.Create(f => f
            .SetMinimumLevel(LogLevel.Trace)
            .ClearProviders()
            .AddDebugger()
        );

        factory
            .CreateLogger<IntegrationTests>()
            .LogInformation(
                "You should see {what} in the {where}.",
                "this",
                "Debug Output window"
            );

        SD.Debugger.Break();
    }
}
