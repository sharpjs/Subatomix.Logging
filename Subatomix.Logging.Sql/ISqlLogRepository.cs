// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Sql;

internal interface ISqlLogRepository : IDisposable
{
    Task<bool> TryEnsureConnectionAsync(
        string?           connectionString,
        CancellationToken cancellation
    );

    Task WriteAsync(
        string                logName,
        IEnumerable<LogEntry> entries,
        TimeSpan              timeout,
        CancellationToken     cancellation
    );
}
