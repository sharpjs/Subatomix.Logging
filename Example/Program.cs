// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Subatomix.Logging.Console;
using Subatomix.Logging.Debugger;

Console.WriteLine("Example App v42.1337");

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

var logger = LoggerFactory
    .Create(b => b
        .SetMinimumLevel(LogLevel.Trace)
        .AddPrettyConsole()
        .AddDebugger()
    )
    .CreateLogger("");

using (var scope = logger.Activity("Example Program").Begin())
{
    logger.LogTrace       ("This is a message.");
    logger.LogDebug       ("This is a message.");
    logger.LogInformation ("This is a message.");
    logger.LogWarning     ("This is a message.");
    logger.LogError       ("This is a message.");
    logger.LogCritical    ("This is a message.");

    try
    {
        throw new Exception("An example exception was thrown.");
    }
    catch (Exception e)
    {
        scope.Exception = e;
    }
}

Console.ReadKey();
