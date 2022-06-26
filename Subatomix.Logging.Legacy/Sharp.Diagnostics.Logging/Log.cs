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

using System.Runtime.ExceptionServices;
using System.Security;
using Microsoft.Extensions.Logging.Abstractions;

namespace Sharp.Diagnostics.Logging;

using F = Formatters;

/// <summary>
///   Convenience methods for logging.
/// </summary>
/// <remarks>
///   This type is a compatibility shim to assist migration from the
///   Sharp.Diagnostics.Logging package. New code should use
///   <see cref="ILogger"/>.
/// </remarks>
public static class Log
{
    #region Logger, Flush, Close

    private static ILogger _logger = NullLogger.Instance;

    /// <summary>
    ///   Gets or sets the <see cref="ILogger"/> instance to which the static
    ///   methods of this class foward invocations.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///   Attempted to set the value of the property to <see langword="null"/>
    /// </exception>
    public static ILogger Logger
    {
        get => _logger;
        set => _logger = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///   Does nothing.
    /// </summary>
    public static void Flush() { }

    /// <summary>
    ///   Does nothing.
    /// </summary>
    public static void Close() { }

    #endregion
    #region Critical

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Critical(string message)
        => Logger.Log(LogLevel.Critical, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Critical(int id, string message)
        => Logger.Log(LogLevel.Critical, id, message, null, F.Message);

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Critical(string format, params object?[] args)
        => Logger.Log(LogLevel.Critical, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Critical(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Critical, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Critical(Exception exception)
        => Logger.Log(LogLevel.Critical, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a critical error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Critical(int id, Exception exception)
        => Logger.Log(LogLevel.Critical, id, default, exception, F.Empty);

    #endregion
    #region Error

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Error(string message)
        => Logger.Log(LogLevel.Error, 0, message, null, F.Message);

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Error(int id, string message)
        => Logger.Log(LogLevel.Error, id, message, null, F.Message);

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Error(string format, params object?[] args)
        => Logger.Log(LogLevel.Error, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Error(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Error, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Error(Exception exception)
        => Logger.Log(LogLevel.Error, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes an error entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Error(int id, Exception exception)
        => Logger.Log(LogLevel.Error, id, default, exception, F.Empty);

    #endregion
    #region Warning

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Warning(string message)
        => Logger.Log(LogLevel.Warning, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Warning(int id, string message)
        => Logger.Log(LogLevel.Warning, id, message, null, F.Message);

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Warning(string format, params object?[] args)
        => Logger.Log(LogLevel.Warning, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Warning(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Warning, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Warning(Exception exception)
        => Logger.Log(LogLevel.Warning, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a warning entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Warning(int id, Exception exception)
        => Logger.Log(LogLevel.Warning, id, default, exception, F.Empty);

    #endregion
    #region Information

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Information(string message)
        => Logger.Log(LogLevel.Information, 0, message, null, F.Message);

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Information(int id, string message)
        => Logger.Log(LogLevel.Information, id, message, null, F.Message);

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Information(string format, params object?[] args)
        => Logger.Log(LogLevel.Information, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Information(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Information, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Information(Exception exception)
        => Logger.Log(LogLevel.Information, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes an informational entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Information(int id, Exception exception)
        => Logger.Log(LogLevel.Information, id, default, exception, F.Empty);

    #endregion
    #region Verbose

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Verbose(string message)
        => Logger.Log(LogLevel.Debug, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Verbose(int id, string message)
        => Logger.Log(LogLevel.Debug, id, message, null, F.Message);

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Verbose(string format, params object?[] args)
        => Logger.Log(LogLevel.Debug, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Verbose(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Debug, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Verbose(Exception exception)
        => Logger.Log(LogLevel.Debug, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a verbose entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Verbose(int id, Exception exception)
        => Logger.Log(LogLevel.Debug, id, default, exception, F.Empty);

    #endregion
    #region Start

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Start(string message)
        => Logger.Log(LogLevel.Information, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Start(int id, string message)
        => Logger.Log(LogLevel.Information, id, message, null, F.Message);

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Start(string format, params object?[] args)
        => Logger.Log(LogLevel.Information, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Start(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Information, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Start(Exception exception)
        => Logger.Log(LogLevel.Information, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a start entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Start(int id, Exception exception)
        => Logger.Log(LogLevel.Information, id, default, exception, F.Empty);

    #endregion
    #region Stop

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Stop(string message)
        => Logger.Log(LogLevel.Information, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Stop(int id, string message)
        => Logger.Log(LogLevel.Information, id, message, null, F.Message);

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Stop(string format, params object?[] args)
        => Logger.Log(LogLevel.Information, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Stop(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Information, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Stop(Exception exception)
        => Logger.Log(LogLevel.Information, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a stop entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Stop(int id, Exception exception)
        => Logger.Log(LogLevel.Information, id, default, exception, F.Empty);

    #endregion
    #region Suspend

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Suspend(string message)
        => Logger.Log(LogLevel.Information, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Suspend(int id, string message)
        => Logger.Log(LogLevel.Information, id, message, null, F.Message);

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Suspend(string format, params object?[] args)
        => Logger.Log(LogLevel.Information, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Suspend(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Information, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Suspend(Exception exception)
        => Logger.Log(LogLevel.Information, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a suspend entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Suspend(int id, Exception exception)
        => Logger.Log(LogLevel.Information, id, default, exception, F.Empty);

    #endregion
    #region Resume

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Resume(string message)
        => Logger.Log(LogLevel.Information, 0, message, null, F.Message);

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Resume(int id, string message)
        => Logger.Log(LogLevel.Information, id, message, null, F.Message);

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Resume(string format, params object?[] args)
        => Logger.Log(LogLevel.Information, 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Resume(int id, string format, params object?[] args)
        => Logger.Log(LogLevel.Information, id, (format, args), null, F.Template);

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Resume(Exception exception)
        => Logger.Log(LogLevel.Information, 0, default, exception, F.Empty);

    /// <summary>
    ///   Writes a resume entry to the log.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="exception">An exception to report in the entry.</param>
    [Conditional("TRACE")]
    public static void Resume(int id, Exception exception)
        => Logger.Log(LogLevel.Information, id, default, exception, F.Empty);

    #endregion
    #region Operations / Correlation

    /// <summary>
    ///   Gets the identifier of the current logical activity.
    /// </summary>
    public static Guid ActivityId
        => Trace.CorrelationManager.ActivityId;

    /// <summary>
    ///   Gets the current stack of logial operations.
    /// </summary>
    /// <returns>
    ///   A new array containing the objects in the logical operation stack,
    ///   ordered from top to bottom.
    /// </returns>
    public static object?[] GetOperationStack()
        => Trace.CorrelationManager.LogicalOperationStack.ToArray();

    /// <summary>
    ///   Starts a logical operation, writing a start entry to the log.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member, if supported by the compiler.
    /// </param>
    /// <returns>
    ///   A <see cref="TraceOperation"/> representing the logical operation.
    ///   When disposed, the object writes stop and error entries to the log.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    public static TraceOperation Operation([CallerMemberName] string name = null!)
        => new(name);

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries to
    ///   the log.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="name"/> and/or <paramref name="action"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    [DebuggerStepThrough]
    public static void Do(string name, Action action)
        => Logger.Activity(name).Do(action);

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries to
    ///   the log.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="name"/> and/or <paramref name="action"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    [DebuggerStepThrough]
    public static Task DoAsync(string name, Func<Task> action)
        => Logger.Activity(name).DoAsync(action);

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries to
    ///   the log.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="name"/> and/or <paramref name="action"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static T Do<T>(string name, Func<T> action)
        => Logger.Activity(name).Do(action);

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries to
    ///   the log.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="name"/> and/or <paramref name="action"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static Task<T> DoAsync<T>(string name, Func<Task<T>> action)
        => Logger.Activity(name).DoAsync(action);

    /// <summary>
    ///   Writes an entry to the log, reporting a new identifier for the
    ///   current logical activity.
    /// </summary>
    /// <param name="message">A message for the entry.</param>
    /// <param name="newActivityId">A new identifier for the current logical activity.</param>
    /// <remarks>
    ///   <para>
    ///     This method is intended for use with the logical activities of a
    ///     <see cref="CorrelationManager"/>.
    ///     The <paramref name="newActivityId"/> parameter relates to the
    ///     <see cref="CorrelationManager.ActivityId"/> property.
    ///   </para>
    ///   <para>
    ///     If a logical operation begins in one activity and transfers to
    ///     another, the second activity should log the transfer by calling
    ///     this method.  The call relates the new activity identifier to the
    ///     previous identifier.  An example consumer of this functionality is
    ///     a trace viewer that can report logical operations spanning multiple
    ///     activities.
    ///   </para>
    /// </remarks>
    [Conditional("TRACE")]
    public static void Transfer(string message, Guid newActivityId)
        => Logger.Log(LogLevel.Information, 0, (message, newActivityId), null, F.Transfer);

    /// <summary>
    ///   Writes an entry to the log, reporting a new identifier for the
    ///   current logical activity.
    /// </summary>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    /// <param name="newActivityId">A new identifier for the current logical activity.</param>
    /// <remarks>
    ///   <para>
    ///     This method is intended for use with the logical activities of a
    ///     <see cref="CorrelationManager"/>.
    ///     The <paramref name="newActivityId"/> parameter relates to the
    ///     <see cref="CorrelationManager.ActivityId"/> property.
    ///   </para>
    ///   <para>
    ///     If a logical operation begins in one activity and transfers to
    ///     another, the second activity should log the transfer by calling
    ///     this method.  The call relates the new activity identifier to the
    ///     previous identifier.  An example consumer of this functionality is
    ///     a trace viewer that can report logical operations spanning multiple
    ///     activities.
    ///   </para>
    /// </remarks>
    [Conditional("TRACE")]
    public static void Transfer(int id, string message, Guid newActivityId)
        => Logger.Log(LogLevel.Information, id, (message, newActivityId), null, F.Transfer);

    #endregion
    #region Event

    /// <summary>
    ///   Writes an entry to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="id">A numeric identifier for the entry.</param>
    [Conditional("TRACE")]
    public static void Event(TraceEventType eventType, int id)
        => Logger.Log(eventType.ToLogLevel(), id, id, null, F.MessageId);

    /// <summary>
    ///   Writes an entry to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Event(TraceEventType eventType, string message)
        => Logger.Log(eventType.ToLogLevel(), 0, message, null, F.Message);

    /// <summary>
    ///   Writes an entry to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="message">A message for the entry.</param>
    [Conditional("TRACE")]
    public static void Event(TraceEventType eventType, int id, string message)
        => Logger.Log(eventType.ToLogLevel(), id, message, null, F.Message);

    /// <summary>
    ///   Writes an entry to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Event(TraceEventType eventType, string format, params object?[] args)
        => Logger.Log(eventType.ToLogLevel(), 0, (format, args), null, F.Template);

    /// <summary>
    ///   Writes an entry to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="format">A format string to build a message for the entry.</param>
    /// <param name="args">The objects to substitute into the format string.</param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="format"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="FormatException">
    ///   <paramref name="format"/> is invalid or
    ///   specifies an argument position not present in <paramref name="args"/>.
    /// </exception>
    [Conditional("TRACE")]
    public static void Event(TraceEventType eventType, int id, string format, params object?[] args)
        => Logger.Log(eventType.ToLogLevel(), id, (format, args), null, F.Template);

    #endregion
    #region Data

    /// <summary>
    ///   Writes arbitrary object data to the log.
    /// </summary>
    /// <param name="eventType">The type of event to write.</param>
    /// <param name="data">The object data to include in the event.</param>
    [Conditional("TRACE")]
    public static void Data(TraceEventType eventType, object? data)
        => Logger.Log(eventType.ToLogLevel(), 0, data, null, F.Data);

    /// <summary>
    ///   Writes arbitrary object data to the log.
    /// </summary>
    /// <param name="eventType">The type of event to write.</param>
    /// <param name="id">A numeric identifier for the event.</param>
    /// <param name="data">The object data to include in the event.</param>
    [Conditional("TRACE")]
    public static void Data(TraceEventType eventType, int id, object? data)
        => Logger.Log(eventType.ToLogLevel(), id, data, null, F.Data);

    /// <summary>
    ///   Writes arbitrary object data to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="data">The object data to include in the entry.</param>
    [Conditional("TRACE")]
    public static void Data(TraceEventType eventType, params object?[]? data)
        => Logger.Log(eventType.ToLogLevel(), 0, data, null, F.DataArray);

    /// <summary>
    ///   Writes arbitrary object data to the log.
    /// </summary>
    /// <param name="eventType">The type of entry to write.</param>
    /// <param name="id">A numeric identifier for the entry.</param>
    /// <param name="data">The object data to include in the entry.</param>
    [Conditional("TRACE")]
    public static void Data(TraceEventType eventType, int id, params object?[]? data)
        => Logger.Log(eventType.ToLogLevel(), id, data, null, F.DataArray);

    #endregion
    #region Event Handlers

    private static bool
        _logAllThrownExceptions,    // true => log first-chance exceptions
        _closeOnExit;               // true => close trace source on exit

    [ThreadStatic]
    private static int
        _inEvent;                   // !0 => thread is in first-chance exception handler

    // NOTE: int instead of bool; Interlocked.CompareExchange does not support bool

    /// <summary>
    ///   Enables or disables logging of <strong>ALL</strong> thrown exceptions
    ///   — even caught exceptions.
    /// </summary>
    /// <remarks>
    ///   ⚠ <strong>WARNING:</strong> This results in an extremely noisy log
    ///   and degrades application performance. Setting this property is not a
    ///   thread-safe operation.
    /// </remarks>
    public static bool LogAllThrownExceptions
    {
        get => _logAllThrownExceptions;
        set
        {
            if (_logAllThrownExceptions == value)
                return;

            var domain = AppDomain.CurrentDomain;

            if (value)
                domain.FirstChanceException += AppDomain_FirstChanceException;
            else
                domain.FirstChanceException -= AppDomain_FirstChanceException;

            _logAllThrownExceptions = value;
        }
    }

    /// <summary>
    ///   Enables or disables logging of application termination and automatic
    ///   closing of attached listeners.
    /// </summary>
    public static bool CloseOnExit
    {
        get => _closeOnExit;
        set
        {
            if (_closeOnExit == value)
                return;

            var domain = AppDomain.CurrentDomain;

            if (value)
            {
                domain.UnhandledException += AppDomain_UnhandledException;
                domain.DomainUnload += AppDomain_DomainUnload;
                domain.ProcessExit += AppDomain_ProcessExit;
            }
            else
            {
                domain.UnhandledException -= AppDomain_UnhandledException;
                domain.DomainUnload -= AppDomain_DomainUnload;
                domain.ProcessExit -= AppDomain_ProcessExit;
            }

            _closeOnExit = value;
        }
    }

#if !NETCOREAPP
    [SecurityCritical, HandleProcessCorruptedStateExceptions]
    // ^^ These attributes opt-in this handler to receive certain severe
    //    exceptions that indicate the process state might be corrupt, such as
    //    stack overflows and access violations.  Unsupported on .NET Core/5+.
#endif
    private static void AppDomain_FirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        // Raised when ANY exception is thrown, either by app code or by
        // framework code, and BEFORE any catch.  These are called
        // 'first-chance' exceptions.  Some will be caught and handled, and as
        // such are not necessarily problems the user needs to know about.
        // They are observed here for diagnostic purposes.

        // This is critical code; make no assumptions.
        if (e is not { Exception: { } exception })
            return;

        // Avoid reentrancy if this method causes its own exceptions.
        // That would result in a stack overflow.
        if (Interlocked.CompareExchange(ref _inEvent, -1, 0) != 0)
            return;

        try
        {
            Logger.LogDebug(exception, "An exception was thrown.");
        }
        catch
        {
            // Logging here is best-effort only.  If the logging API throws
            // an exception here, there is no choice but to ignore it.  The
            // original exception that fired this event must be allowed to
            // continue, so that it can be caught and handled.
        }

        Interlocked.Exchange(ref _inEvent, 0);
    }

#if !NETCOREAPP
    [SecurityCritical, HandleProcessCorruptedStateExceptions]
    // ^^ These attributes opt-in this handler to receive certain severe
    //    exceptions that indicate the process state might be corrupt, such as
    //    stack overflows and access violations.  Unsupported on .NET Core/5+.
#endif
    private static void AppDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        // Raised when neither app code nor any framework code handles an
        // exception, except for a few special cases which go unnoticed:
        //
        // - ThreadAbortException when the thread's Abort method is invoked
        //     (not in .NET Core/5+)
        // - AppDomainUnloadedException when the thread's application domain
        //     is being unloaded
        // - CLR internal exceptions

        // This is critical code; make no assumptions.
        if (e is not { ExceptionObject: { } exceptionObject })
            return;

        try
        {
            var type = exceptionObject.GetType().FullName;
            var exception = exceptionObject as Exception;

            if (e.IsTerminating)
            {
                Logger.LogCritical(
                    "Terminating due to an unhandled exception of type {exceptionType}.",
                    type
                );

                if (exception != null)
                    Logger.LogCritical(exception);
            }
            else
            {
                Logger.LogError(
                    "Unhandled exception of type {exceptionType}. Execution will continue.",
                    type
                );

                if (exception != null)
                    Logger.LogError(exception);
            }
        }
        catch
        {
            // Logging here is best-effort only.  If the logging API throws
            // an exception here, there is no choice but to ignore it.  The
            // runtime unhandled-exception behavior must continue, and the
            // original exception must not be obscured by a secondary one.
        }
    }

    private static void AppDomain_DomainUnload(object? sender, EventArgs e)
    {
        // Raised in a non-default application domain when its Unload method
        // is invoked.  NEVER raised in the default application domain.
        // Unsupported on .NET Core/5+.

        var name = sender is AppDomain a ? a.FriendlyName : "unknown";

        Logger.LogInformation("The AppDomain '{appDomainName}' is unloading.", name);
    }

    private static void AppDomain_ProcessExit(object? sender, EventArgs e)
    {
        // Raised in every application domain (that registers an event handler)
        // when the hosting process is exiting normally.

        var name = sender is AppDomain a ? a.FriendlyName : "unknown";

        Logger.LogInformation("The parent process of AppDomain '{appDomainName}' is exiting.", name);
    }

    internal static void SimulateFirstChanceException(FirstChanceExceptionEventArgs e)
    {
        AppDomain_FirstChanceException(AppDomain.CurrentDomain, e);
    }

    internal static void SimulateUnhandledException(UnhandledExceptionEventArgs e)
    {
        AppDomain_UnhandledException(AppDomain.CurrentDomain, e);
    }

    internal static void SimulateDomainUnload(object domain)
    {
        AppDomain_DomainUnload(domain, EventArgs.Empty);
    }

    internal static void SimulateProcessExit(object domain)
    {
        AppDomain_ProcessExit(domain, EventArgs.Empty);
    }

    #endregion
}
