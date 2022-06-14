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

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

using LE = Microsoft.Extensions.Logging.LoggerExtensions;

namespace Subatomix.Logging;

using static MethodImplOptions;

#if NETFRAMEWORK
// Prevent CS1574: XML comment has cref attribute '...' that could not be resolved
using LogLevel = LogLevel;
#endif

#if NET6_0_OR_GREATER
// This warning is for downstream code and is not applicable to extension methods on ILogger
#pragma warning disable CA2254 // The logging message template should not vary between calls...
#endif

/// <summary>
///   Extension methods for <see cref="ILogger"/>.
/// </summary>
public static class LoggerExtensions
{
    #region Trace

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Trace"/>
    ///   (very verbose) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogTrace(ILogger, string?, object?[])">LogTrace</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Trace(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Trace, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Trace"/>
    ///   (very verbose) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogTrace(ILogger, Exception?, string?, object?[])">LogTrace</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Trace(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Trace, exception);

    #endregion
    #region Debug

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Debug"/>
    ///   (verbose) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogDebug(ILogger, string?, object?[])">LogDebug</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Debug(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Debug, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Debug"/>
    ///   (verbose) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogDebug(ILogger, Exception?, string?, object?[])">LogDebug</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Debug(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Debug, exception);

    #endregion
    #region Info

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Information"/>
    ///   level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogInformation(ILogger, string?, object?[])">LogInformation</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Info(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Information, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Information"/>
    ///   level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogInformation(ILogger, Exception?, string?, object?[])">LogInformation</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Info(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Information, exception);

    #endregion
    #region Warn

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogWarning(ILogger, string?, object?[])">LogWarning</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Warn(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Warning, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Warning"/> level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogWarning(ILogger, Exception?, string?, object?[])">LogWarning</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Warn(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Warning, exception);

    #endregion
    #region Error

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogError(ILogger, string?, object?[])">LogError</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Error(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Error, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogError(ILogger, Exception?, string?, object?[])">LogError</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Error(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Error, exception);

    #endregion
    #region Critical

    /// <summary>
    ///   Logs the specified message at <see cref="LogLevel.Critical"/>
    ///   (fatal) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="message">
    ///   The template for the message to log.  Can contain placeholders.
    /// </param>
    /// <param name="args">
    ///   An array of values to substitute into the placeholders of
    ///   <paramref name="message"/>, one element per placeholder.
    ///   The order of placeholders, not their names, determines the order of
    ///   substitutions.
    /// </param>
    /// <remarks>
    ///   <para>
    ///     This method is a more concise synonym for
    ///     <see cref="LE.LogCritical(ILogger, string?, object?[])">LogCritical</see>.
    ///   </para>
    ///   <para>
    ///     Use <strong>names</strong> for the placeholders, not numbers.
    ///     For example: <c>"User {user} logged in from {address}."</c>
    ///   </para>
    ///   <para>
    ///     <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#log-message-template">More information</a>
    ///   </para>
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Critical(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Critical, message, args);

    /// <summary>
    ///   Logs the specified exception at <see cref="LogLevel.Critical"/>
    ///   (fatal) level.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    /// <remarks>
    ///   This method is a more concise synonym for
    ///   <see cref="LE.LogCritical(ILogger, Exception?, string?, object?[])">LogCritical</see>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static void Critical(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Critical, exception);

    #endregion
    #region Log

    /// <summary>
    ///   Logs the specified exception.
    /// </summary>
    /// <param name="logger">
    ///   The logger to use.
    /// </param>
    /// <param name="level">
    ///   The severity level of the exception
    /// </param>
    /// <param name="exception">
    ///   The exception to log.
    /// </param>
    [MethodImpl(AggressiveInlining)]
    public static void Log(this ILogger logger, LogLevel level, Exception exception)
        => logger.Log(level, 0, default, exception, EmptyFormatter);

    #endregion
    #region Scopes

    /// <summary>
    ///   Begins a logical operation scope that automatically logs start,
    ///   completion, and (optional) exception messages.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   A scope representing the logical operation.
    ///   See <see cref="OperationScope"/> for details.
    /// </returns>
    /// <remarks>
    ///   This overload uses the <see cref="LogLevel.Information"/> severity
    ///   level for start and completion messages.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static OperationScope BeginOperation(
        this ILogger              logger,
        [CallerMemberName] string name = null!)
        => logger.BeginOperation(LogLevel.Information, name);

    /// <summary>
    ///   Begins a logical operation scope that automatically logs start,
    ///   completion, and (optional) exception messages.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="logLevel">
    ///   The severity level for operation start and completion messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   A scope representing the logical operation.
    ///   See <see cref="OperationScope"/> for details.
    /// </returns>
    [MethodImpl(AggressiveInlining)]
    public static OperationScope BeginOperation(
        this ILogger              logger,
        LogLevel                  logLevel,
        [CallerMemberName] string name = null!)
        => new(logger, logLevel, name);

    /// <summary>
    ///   Begins a logical operation scope that automatically logs start,
    ///   completion, and (optional) exception messages and automatically
    ///   starts and stops a named <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   A scope representing the logical operation.
    ///   See <see cref="ActivityScope"/> and <see cref="OperationScope"/> for
    ///   details.
    /// </returns>
    /// <remarks>
    ///   This overload uses the <see cref="LogLevel.Information"/> severity
    ///   level for start and completion messages.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static ActivityScope BeginActivity(
        this ILogger              logger,
        [CallerMemberName] string name = null!)
        => logger.BeginActivity(LogLevel.Information, name);

    /// <summary>
    ///   Begins a logical operation scope that automatically logs start,
    ///   completion, and (optional) exception messages and automatically
    ///   starts and stops a named <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="logLevel">
    ///   The severity level for operation start and completion messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   A scope representing the logical operation.
    ///   See <see cref="ActivityScope"/> and <see cref="OperationScope"/> for
    ///   details.
    /// </returns>
    [MethodImpl(AggressiveInlining)]
    public static ActivityScope BeginActivity(
        this ILogger              logger,
        LogLevel                  logLevel,
        [CallerMemberName] string name = null!)
        => new(logger, logLevel, name);

    #endregion
    #region Formatting

    private static readonly Func<Void, Exception?, string>
        EmptyFormatter = FormatEmpty;

    internal static string FormatEmpty(Void state, Exception? exception)
        => string.Empty;

    #endregion
}
