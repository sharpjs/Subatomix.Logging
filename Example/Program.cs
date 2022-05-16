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
using Microsoft.Extensions.Logging;
using Subatomix.Diagnostics.Console;

Console.WriteLine("Example App v42.1337");

Activity.DefaultIdFormat = ActivityIdFormat.W3C;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);

#if IS_THIS_NEEDED
    builder.Configure(options =>
    {
        options.ActivityTrackingOptions
            = ActivityTrackingOptions.TraceId;
    });
#endif

    builder.AddConsoleFormatter<PrettyConsoleFormatter, PrettyConsoleFormatterOptions>();

    builder.AddConsole(console =>
    {
        console.FormatterName = "pretty";
    });
});

using var activity = new Activity("Run").Start();

var logger = loggerFactory.CreateLogger("");

logger.LogTrace       ("This is a message.");
logger.LogDebug       ("This is a message.");
logger.LogInformation ("This is a message.");
logger.LogWarning     ("This is a message.");
logger.LogError       ("This is a message.");
logger.LogCritical    ("This is a message.");

Console.ReadKey();
