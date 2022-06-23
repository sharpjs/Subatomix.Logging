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

namespace Subatomix.Logging;

using Base = MEL.LoggerExtensions;

/// <summary>
///   Extension methods for <see cref="ILogger"/>.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Trace"/>
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
    ///   <see cref="Base.LogTrace(ILogger, Exception?, string?, object?[])">LogTrace</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogTrace(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Trace, exception);

    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Debug"/>
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
    ///   <see cref="Base.LogDebug(ILogger, Exception?, string?, object?[])">LogDebug</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Debug, exception);

    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Information"/>
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
    ///   <see cref="Base.LogInformation(ILogger, Exception?, string?, object?[])">LogInformation</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Information, exception);

    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Warning"/>
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
    ///   <see cref="Base.LogWarning(ILogger, Exception?, string?, object?[])">LogWarning</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Warning, exception);

    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Error"/>
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
    ///   <see cref="Base.LogError(ILogger, Exception?, string?, object?[])">LogError</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Error, exception);

    /// <summary>
    ///   Logs the specified exception at <see cref="MEL.LogLevel.Critical"/>
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
    ///   <see cref="Base.LogCritical(ILogger, Exception?, string?, object?[])">LogCritical</see>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogCritical(this ILogger logger, Exception exception)
        => logger.Log(LogLevel.Critical, exception);

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(this ILogger logger, LogLevel level, Exception exception)
        => logger.Log(level, 0, default, exception, EmptyFormatter);

    /// <summary>
    ///   Prepares to create a logical operation scope that automatically logs
    ///   start, completion, and (optional) exception messages.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   An object that exposes methods to begin or to perform an operation in
    ///   an <see cref="OperationScope"/> with the specified
    ///   <paramref name="logger"/> and <paramref name="name"/>.
    /// </returns>
    /// <remarks>
    ///   This overload uses the <see cref="MEL.LogLevel.Information"/>
    ///   severity level for start and completion messages.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationScopeInitiator Operation(
        this ILogger              logger,
        [CallerMemberName] string name = null!)
        => logger.Operation(LogLevel.Information, name);

    /// <summary>
    ///   Prepares to create a logical operation scope that automatically logs
    ///   start, completion, and (optional) exception messages.
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
    ///   An object that exposes methods to begin or to perform an operation in
    ///   an <see cref="OperationScope"/> with the specified
    ///   <paramref name="logger"/>, <paramref name="logLevel"/>, and
    ///   <paramref name="name"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OperationScopeInitiator Operation(
        this ILogger              logger,
        LogLevel                  logLevel,
        [CallerMemberName] string name = null!)
        => new(logger, logLevel, name);

    /// <summary>
    ///   Prepares to create a logical operation scope that automatically logs
    ///   start, completion, and (optional) exception messages and
    ///   automatically starts and stops a named
    ///   <see cref="System.Diagnostics.Activity"/>.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    /// <returns>
    ///   An object that exposes methods to begin or to perform an operation in
    ///   an <see cref="ActivityScope"/> with the specified
    ///   <paramref name="logger"/> and <paramref name="name"/>.
    /// </returns>
    /// <remarks>
    ///   This overload uses the <see cref="MEL.LogLevel.Information"/>
    ///   severity level for start and completion messages.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActivityScopeInitiator Activity(
        this ILogger              logger,
        [CallerMemberName] string name = null!)
        => logger.Activity(LogLevel.Information, name);

    /// <summary>
    ///   Prepares to create a logical operation scope that automatically logs
    ///   start, completion, and (optional) exception messages and
    ///   automatically starts and stops a named
    ///   <see cref="System.Diagnostics.Activity"/>.
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
    ///   An object that exposes methods to begin or to perform an operation in
    ///   an <see cref="ActivityScope"/> with the specified
    ///   <paramref name="logger"/>, <paramref name="logLevel"/>, and
    ///   <paramref name="name"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActivityScopeInitiator Activity(
        this ILogger              logger,
        LogLevel                  logLevel,
        [CallerMemberName] string name = null!)
        => new(logger, logLevel, name);

    private static readonly Func<Void, Exception?, string>
        EmptyFormatter = FormatEmpty;

    internal static string FormatEmpty(Void state, Exception? exception)
        => string.Empty;
}
