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

using FluentAssertions.Extensions;

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
    }

    [Test]
    public void TryEnsureConnection_Null()
    {
        using var repository = new SqlLogRepository();

        repository.TryEnsureConnection(null);

        repository.IsConnected.Should().BeFalse();
    }

    [Test]
    [Category("Integration")]
    [Explicit("Requires local SQL Server instance.")]
    public void TryEnsureConnection_Local()
    {
        using var repository = new SqlLogRepository();

        repository.TryEnsureConnection("Server=.;Database=LogTest;Integrated Security=True;Trust Server Certificate=True");

        repository.IsConnected.Should().BeTrue();
    }

    [Test]
    public void Write_NotConnected()
    {
        using var repository = new SqlLogRepository();

        repository.Write("test", Array.Empty<LogEntry>(), 10.Seconds());
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        using var repository = new SqlLogRepository();

        repository.SimulateUnmanagedDisposal();
    }
}
