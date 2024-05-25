// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Sql;

using Subatomix.Logging.Internal;
using static ActivityIdFormat;

/// <summary>
///   A logger that writes messages to Azure SQL Database, SQL Server, or a
///   compatible database product.
/// </summary>
public class SqlLogger : ILogger
{
    internal SqlLogger(ISqlLoggerProvider provider, string name)
    {
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        Name     = name     ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    ///   Gets the associated provider.
    /// </summary>
    internal ISqlLoggerProvider Provider { get; }

    /// <summary>
    ///   Gets the category name.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
        => true;

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state)
        => NullScope.Instance;

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel                         logLevel,
        EventId                          eventId,
        TState                           state,
        Exception?                       exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = Combine(
            formatter?.Invoke(state, exception),
            exception?.ToString()
        );

        if (message is not { Length: > 0 })
            return;

        Provider.Enqueue(new()
        {
            Date     = DateTime.UtcNow,
            Category = Name,
            Level    = logLevel,
            EventId  = eventId.Id,
            TraceId  = GetTraceId(),
            Message  = message
        });
    }

    private static string? Combine(string? a, string? b)
    {
        if (a is not { Length: > 0 })
            return b;

        if (b is not { Length: > 0 })
            return a;

        return string.Concat(a, " ", b);
    }

    private static string? GetTraceId()
    {
        return Activity.Current switch
        {
            { IdFormat: W3C          } a => a.TraceId.ToString(),
            { IdFormat: Hierarchical } a => a.RootId,
            _                            => null
        };
    }
}
