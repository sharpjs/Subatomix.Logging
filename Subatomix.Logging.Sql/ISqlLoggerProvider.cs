// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Sql;

internal interface ISqlLoggerProvider
{
    void Enqueue(LogEntry entry);
}
