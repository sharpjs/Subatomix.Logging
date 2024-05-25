// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Legacy;

internal static class TraceEventTypeExtensions
{
    public static LogLevel ToLogLevel(this TraceEventType type)
    {
        return type switch
        {
            TraceEventType.Critical    => LogLevel.Critical,
            TraceEventType.Error       => LogLevel.Error,
            TraceEventType.Warning     => LogLevel.Warning,
            TraceEventType.Information => LogLevel.Information,
            TraceEventType.Verbose     => LogLevel.Debug,
            _                          => LogLevel.Information
        };
    }
}
