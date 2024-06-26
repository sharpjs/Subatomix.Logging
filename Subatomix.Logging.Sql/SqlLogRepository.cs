// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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

    private string? _connectionString;

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
        _connectionString = null;

        // Require connection string
        if (connectionString is not { Length: > 0 })
            return false;

        // Set up new connection
        connection = new(connectionString);
        Connection = connection;
        _connectionString = connectionString;

        await connection.OpenAsync(cancellation);
        return true;
    }

    public async Task WriteAsync(
        string                logName,
        IEnumerable<LogEntry> entries,
        TimeSpan              timeout,
        CancellationToken     cancellation)
    {
        if (logName is null)
            throw new ArgumentNullException(nameof(logName));
        if (entries is null)
            throw new ArgumentNullException(nameof(entries));

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

    private bool IsConnectedCore(
        [NotNullWhen(true)] SqlConnection? connection,
        string? connectionString)
    {
        return IsConnectedCore(connection)
            && _connectionString == connectionString;
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
        _connectionString = null;
    }
}
