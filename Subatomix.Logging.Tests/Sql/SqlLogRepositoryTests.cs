// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Data;
using System.Transactions;
using FluentAssertions.Extensions;
using Microsoft.Data.SqlClient;

namespace Subatomix.Logging.Sql;

[TestFixture]
public class SqlLogRepositoryTests
{
    [Test]
    public void InitialState()
    {
        using var repository = new SqlLogRepository();

        repository.MachineName.Should().Be(Environment.MachineName);
        repository.ProcessId  .Should().Be(Process.GetCurrentProcess().Id);
        repository.IsConnected.Should().BeFalse();
        repository.Connection .Should().BeNull();
    }

    [Test]
    public async Task TryEnsureConnectionAsync_NullConnectionString()
    {
        using var repository = new SqlLogRepository();

        await repository.TryEnsureConnectionAsync(null, default);

        repository.IsConnected.Should().BeFalse();
        repository.Connection .Should().BeNull();
    }

    [Test]
    [Category("Integration")]
    public async Task TryEnsureConnectionAsync_Normal()
    {
        using var repository = new SqlLogRepository();

        await repository.TryEnsureConnectionAsync(ConnectionString, default);

        repository.IsConnected.Should().BeTrue();
        repository.Connection .Should().NotBeNull();
    }

    [Test]
    [Category("Integration")]
    public async Task TryEnsureConnectionAsync_SameConnectionString()
    {
        using var repository = new SqlLogRepository();

        await repository.TryEnsureConnectionAsync(ConnectionString, default);
        var connectionA = repository.Connection;

        await repository.TryEnsureConnectionAsync(ConnectionString, default);
        var connectionB = repository.Connection;

        repository.IsConnected.Should().BeTrue();
        repository.Connection .Should().NotBeNull();

        connectionA.Should().BeSameAs(connectionB);
    }

    [Test]
    [Category("Integration")]
    public async Task TryEnsureConnectionAsync_DifferentConnectionString()
    {
        using var repository = new SqlLogRepository();

        await repository.TryEnsureConnectionAsync(ConnectionString, default);
        var connectionA = repository.Connection;

        await repository.TryEnsureConnectionAsync(ConnectionString + ";Pooling=False", default);
        var connectionB = repository.Connection;

        repository.IsConnected.Should().BeTrue();
        repository.Connection .Should().NotBeNull();

        connectionA.Should().NotBeSameAs(connectionB);
    }

    [Test]
    public async Task Write_NullLogName()
    {
        using var repository = new SqlLogRepository();

        await repository
            .Awaiting(r => r.WriteAsync(null!, new LogEntry[0], 10.Seconds(), default))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("logName");
    }

    [Test]
    public async Task Write_NullEntries()
    {
        using var repository = new SqlLogRepository();

        await repository
            .Awaiting(r => r.WriteAsync("any", null!, 10.Seconds(), default))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("entries");
    }

    [Test]
    public async Task Write_NotConnected()
    {
        using var repository = new SqlLogRepository();

        var entries = new LogEntry[] { new() { } };

        // does nothing
        await repository.WriteAsync(Any.GetString(), entries, 10.Seconds(), default);
    }

    [Test]
    [Category("Integration")]
    public async Task Write_Normal()
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var repository  = new SqlLogRepository();

        await repository.TryEnsureConnectionAsync(ConnectionString, default);

        repository.IsConnected.Should().BeTrue();

        var now         = DateTime.UtcNow;
        var machineName = Environment.MachineName;
        var processId   = Process.GetCurrentProcess().Id;
        var logName     = Any.GetString( 128 + 1);

        var entries = new LogEntry[]
        {
            new()
            {
                Date     = now,
                Ordinal  = 0,
                TraceId  = null,
                EventId  = null,
                Level    = LogLevel.Trace,
                Message  = "",
                Category = "",
            },
            new()
            {
                Date     = now,
                Ordinal  = 1,
                TraceId  = Any.GetString(  32 + 1),
                EventId  = Any.Next(),
                Level    = LogLevel.Critical,
                Message  = Any.GetString(1024 + 1),
                Category = Any.GetString( 128 + 1),
            },
        };

        await repository.WriteAsync(logName, entries, timeout: 10.Seconds(), default);

        const string SqlTemplate
            = "SELECT Entry.*, MachineName = Machine.Name"             + "{0}"
            + "FROM log.Entry"                                         + "{0}"
            + "INNER JOIN log.Log     ON Log    .Id = Entry.LogId"     + "{0}"
            + "INNER JOIN log.Machine ON Machine.Id = Entry.MachineId" + "{0}"
            + "WHERE Log.Name = '{1}';"                                + "{0}"
            ;

        var sql = string.Format(
            SqlTemplate,
            Environment.NewLine,
            logName.Substring(0, 128).Replace("'", "''")
        );

        using var command = new SqlCommand(sql, repository.Connection!);
        using var reader  = await command.ExecuteReaderAsync(CommandBehavior.SingleResult);

        (await reader.ReadAsync()).Should().BeTrue();

        reader["Date"]       .Should().BeOfType<DateTime>().Which
                             .Should().BeCloseTo(now, precision: 100.Microseconds());
        reader["MachineName"].Should().Be(machineName);
        reader["ProcessId"]  .Should().Be(processId);
        reader["TraceId"]    .Should().Be(DBNull.Value);
        reader["EventId"]    .Should().Be(DBNull.Value);
        reader["Level"]      .Should().Be(0);
        reader["Message"]    .Should().Be("");
        reader["Category"]   .Should().Be("");

        (await reader.ReadAsync()).Should().BeTrue();

        reader["Date"]       .Should().BeOfType<DateTime>().Which
                             .Should().BeCloseTo(now, precision: 100.Microseconds());
        reader["MachineName"].Should().Be(machineName);
        reader["ProcessId"]  .Should().Be(processId);
        reader["TraceId"]    .Should().Be(entries[1].TraceId!.Substring(0,   32));
        reader["EventId"]    .Should().Be(entries[1].EventId);
        reader["Level"]      .Should().Be(5);
        reader["Message"]    .Should().Be(entries[1].Message .Substring(0, 1024));
        reader["Category"]   .Should().Be(entries[1].Category.Substring(0,  128));

        (await reader.ReadAsync()).Should().BeFalse();
    }

    [Test]
    public async Task WriteAsync_NotConnected()
    {
        using var repository = new SqlLogRepository();

        await repository.WriteAsync("test", Array.Empty<LogEntry>(), 10.Seconds(), default);
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var repository = new SqlLogRepository();

        repository.SimulateUnmanagedDisposal();
    }
}
