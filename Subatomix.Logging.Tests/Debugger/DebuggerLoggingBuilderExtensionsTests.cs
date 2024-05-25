// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.Extensions.DependencyInjection;

namespace Subatomix.Logging.Debugger;

[TestFixture]
public class DebuggerLoggingBuilderExtensionsTests
{
    [Test]
    public void AddDebugger_NullBuilder()
    {
        Invoking(() => default(ILoggingBuilder)!.AddDebugger())
            .Should().Throw<ArgumentNullException>().WithParameterName("builder");
    }

    [Test]
    public void AddDebugger_Normal()
    {
        var logger = new ServiceCollection()
            .AddLogging(l => l.AddDebugger())
            .BuildServiceProvider()
            .GetRequiredService<ILoggerProvider>()
            .CreateLogger("");

        logger.Should().BeOfType<DebuggerLogger>();
    }
}
