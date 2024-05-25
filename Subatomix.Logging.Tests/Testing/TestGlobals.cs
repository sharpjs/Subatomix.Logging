// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text;
using Microsoft.Data.SqlClient;
using NUnit.Framework.Internal;

namespace Subatomix.Testing;

using static Environment;

/// <summary>
///   Convenience items available globally in tests.
/// </summary>
internal static class TestGlobals
{
    /// <summary>
    ///   Gets the NUnit <see cref="Randomizer"/> for the current
    ///   <see cref="TestContext"/>.
    /// </summary>
    public static Randomizer Any => TestContext.CurrentContext.Random;

    /// <summary>
    ///   Gets a random <see cref="LogLevel"/> except
    ///   <see cref="LogLevel.None"/>.
    /// </summary>
    /// <param name="any">
    ///   A NUnit <see cref="Randomizer"/>.
    /// </param>
    public static LogLevel LogLevelExceptNone(this Randomizer any)
        => (LogLevel) any.Next(
            minValue: (int) LogLevel.Trace, // inclusive
            maxValue: (int) LogLevel.None   // exclusive
        );

    /// <summary>
    ///   Appends <see cref="NewLine"/> to the specified string.
    /// </summary>
    /// <param name="line">
    ///   The string to which to append <see cref="NewLine"/>.
    /// </param>
    public static string Lines(string line)
    {
        return line + NewLine;
    }

    /// <summary>
    ///   Concatenates the specified strings, appending
    ///   <see cref="Environment.NewLine"/> to each.
    /// </summary>
    /// <param name="lines">
    ///   The strings to concatenate.
    /// </param>
    public static string Lines(params string[] lines)
    {
        var length = 0;

        foreach (var line in lines)
            length += line.Length + NewLine.Length;

        var sb = new StringBuilder(length);

        foreach (var line in lines)
            sb.AppendLine(line);

        return sb.ToString();
    }

    /// <summary>
    ///   Gets the connection string to use for database tests.
    /// </summary>
    public static string ConnectionString { get; } = GetConnectionString();

    private static string GetConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource             = ".",
            InitialCatalog         = "LogTest",
            TrustServerCertificate = true,
            ApplicationName        = typeof(TestGlobals).Assembly.GetName().Name
        };

        if (GetEnvironmentVariable("MSSQL_SA_PASSWORD") is { Length: > 0 } password)
        {
            builder.UserID   = "sa";
            builder.Password = password;
        }
        else
        {
            builder.IntegratedSecurity = true;
        }

        return builder.ConnectionString;
    }
}
