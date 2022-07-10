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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace Subatomix.Logging.Sql;

internal class SqlLogRepository : ISqlLogRepository
{
    public SqlLogRepository()
    {
        MachineName = Environment.MachineName.Truncate(255);
        ProcessId   = Process.GetCurrentProcess().Id;
    }

    public string MachineName { get; }

    public int ProcessId { get; }

    public bool IsConnected => IsConnectedCore(Connection);

    // For testing
    internal SqlConnection? Connection { get; private set; }

    public async Task<bool> TryEnsureConnectionAsync(
        string?           connectionString,
        CancellationToken cancellation)
    {
        var connection = Connection;

        // If connection is open and current, use it
        if (IsConnectedCore(connection, connectionString))
            return true;

        // Connection is broken, stale, or null; dispose it
        connection?.Dispose();
        Connection = null;

        // Require connection string
        if (connectionString is not { Length: > 0 })
            return false;

        // Set up new connection
        connection = new(connectionString);
        Connection = connection;

        await connection.OpenAsync(cancellation);
        return true;
    }

    public async Task WriteAsync(
        string                logName,
        IEnumerable<LogEntry> entries,
        TimeSpan              timeout,
        CancellationToken     cancellation)
    {
        if (Connection is not { } connection)
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
            Value = logName.Truncate(128)
        });

        command.Parameters.Add(new("@MachineName", SqlDbType.VarChar, 255)
        {
            Value = MachineName
        });

        command.Parameters.Add(new("@ProcessId", SqlDbType.Int)
        {
            Value = ProcessId
        });

        command.Parameters.Add(new("@EntryRows", SqlDbType.Structured)
        {
            TypeName = "log.EntryRow",
            Value    = new ObjectDataReader<LogEntry>(entries, LogEntry.Map)
        });

        await command.ExecuteNonQueryAsync(cancellation);
    }

    private static bool IsConnectedCore(
        [NotNullWhen(true)] SqlConnection? connection)
    {
        return connection is { State: ConnectionState.Open };
    }

    private static bool IsConnectedCore(
        [NotNullWhen(true)] SqlConnection? connection,
        string? connectionString)
    {
        return IsConnectedCore(connection)
            && connection.ConnectionString == connectionString;
    }

    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    internal void SimulateUnmanagedDisposal()
    {
        Dispose(managed: false);
    }

    protected virtual void Dispose(bool managed)
    {
        if (!managed)
            return;

        Connection?.Dispose();
        Connection = null;
    }
}
