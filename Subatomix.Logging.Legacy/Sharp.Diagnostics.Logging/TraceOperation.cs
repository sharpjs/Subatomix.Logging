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

namespace Sharp.Diagnostics.Logging;

/// <summary>
///   Represents a logical operation whose start and end are logged.
/// </summary>
/// <remarks>
///   This type is a partial compatibility shim to assist migration from the
///   Sharp.Diagnostics.Logging package. New code should use
///   <see cref="ILogger"/> and one of the <c>Activity</c> or <c>Operation</c>
///   extension methods provided by <see cref="SL.LoggerExtensions"/>.
/// </remarks>
public class TraceOperation : ActivityScope
{
    private const string DefaultName = "Operation";

    /// <summary>
    ///   Initializes a new <see cref="TraceOperation"/> instance with the
    ///   specified operation name.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    public TraceOperation([CallerMemberName] string? name = null)
        : this(null, name) { }

    /// <summary>
    ///   Initializes a new <see cref="TraceOperation"/> instance with the
    ///   specified trace source and operation name.
    /// </summary>
    /// <param name="trace">
    ///   Not used.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.  If omitted, the default is the name of
    ///   the calling member.
    /// </param>
    public TraceOperation(TraceSource? trace, [CallerMemberName] string? name = null)
        : base(Log.Logger, LogLevel.Information, name ?? DefaultName)
    { }

    /// <summary>
    ///   Gets the UTC time when the operation started.
    /// </summary>
    public DateTime StartTime => Activity.StartTimeUtc;

    /// <summary>
    ///   Gets the duration elapsed since the operation started.
    /// </summary>
    public TimeSpan ElapsedTime => Duration;

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static void Do(string? name, Action action)
    {
        Do(null, name, action);
    }

    /// <summary>
    ///   Runs a logical operation asynchronously, writing start, stop, and
    ///   error entries.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static Task DoAsync(string? name, Func<Task> action)
    {
        return DoAsync(null, name, action);
    }

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries.
    /// </summary>
    /// <param name="trace">
    ///   Not used.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static void Do(TraceSource? trace, string? name, Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var operation = new TraceOperation(trace, name);

        try
        {
            action();
        }
        catch (Exception e)
        {
            operation.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Runs a logical operation asynchronously, writing start, stop, and
    ///   error entries.
    /// </summary>
    /// <param name="trace">
    ///   Not used.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static async Task DoAsync(TraceSource? trace, string? name, Func<Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var operation = new TraceOperation(trace, name);

        try
        {
            await action();
        }
        catch (Exception e)
        {
            operation.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static TResult Do<TResult>(string? name, Func<TResult> action)
    {
        return Do(null, name, action);
    }

    /// <summary>
    ///   Runs a logical operation asynchronously, writing start, stop, and
    ///   error entries.
    /// </summary>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static Task<TResult> DoAsync<TResult>(string? name, Func<Task<TResult>> action)
    {
        return DoAsync(null, name, action);
    }

    /// <summary>
    ///   Runs a logical operation, writing start, stop, and error entries.
    /// </summary>
    /// <param name="trace">
    ///   Not used.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static TResult Do<TResult>(TraceSource? trace, string? name, Func<TResult> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var operation = new TraceOperation(trace, name);

        try
        {
            return action();
        }
        catch (Exception e)
        {
            operation.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Runs a logical operation asynchronously, writing start, stop, and
    ///   error entries.
    /// </summary>
    /// <param name="trace">
    ///   Not used.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="action">
    ///   The operation.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [DebuggerStepThrough]
    public static async Task<TResult> DoAsync<TResult>(
        TraceSource? trace, string? name, Func<Task<TResult>> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var operation = new TraceOperation(trace, name);

        try
        {
            return await action();
        }
        catch (Exception e)
        {
            operation.Exception = e;
            throw;
        }
    }
}
