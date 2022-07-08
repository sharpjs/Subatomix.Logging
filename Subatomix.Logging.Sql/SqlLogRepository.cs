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

using System.Data;
using Microsoft.Data.SqlClient;

namespace Subatomix.Logging.Sql;

internal class SqlLogRepository : ISqlLogRepository
{
    private SqlConnection? _connection;

    public bool TryEnsureConnection(string? connectionString)
    {
        var connection = _connection;

        // If connection is open and current, use it
        if (connection is { State: ConnectionState.Open })
            if (connection.ConnectionString == connectionString)
                return true;

        // Connection is broken, stale, or null; dispose it
        connection?.Dispose();
        _connection = null;

        // Require connection string
        if (connectionString is not { Length: > 0 })
            return false;

        // Set up new connection
        connection  = new(connectionString);
        _connection = connection;

        connection.Open();
        return true;
    }

    public void Write(string logName, IEnumerable<LogEntry> entries, TimeSpan timeout)
    {
        if (_connection is not { } connection)
            return;

        using var command = new SqlCommand()
        {
            Connection     = connection,
            CommandType    = CommandType.StoredProcedure,
            CommandText    = "log.Write",
            CommandTimeout = (int) timeout.TotalSeconds
        };

        command.Parameters.Add(new("@LogName", SqlDbType.VarChar, 128)
        {
            Value = logName
        });

        command.Parameters.Add(new("@MachineName", SqlDbType.VarChar, 255)
        {
            Value = Environment.MachineName.Truncate(255)
        });

        command.Parameters.Add(new("@ProcessId", SqlDbType.Int)
        {
            Value = Process.GetCurrentProcess().Id
        });

        command.Parameters.Add(new("@EntryRows", SqlDbType.Structured)
        {
            TypeName = "log.EntryRow",
            Value    = new ObjectDataReader<LogEntry>(entries, LogEntry.Map)
        });

        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        _connection?.Dispose();
        _connection = null;
    }
}
