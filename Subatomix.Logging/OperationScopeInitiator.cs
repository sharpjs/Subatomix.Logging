using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Subatomix.Logging;

/// <summary>
///   Begins or performs operations in an <see cref="OperationScope"/>.
/// </summary>
public readonly struct OperationScopeInitiator
{
    private readonly ILogger  _logger;
    private readonly LogLevel _logLevel;
    private readonly string   _name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal OperationScopeInitiator(ILogger logger, LogLevel logLevel, string name)
    {
        _logger   = logger;
        _logLevel = logLevel;
        _name     = name;
    }

    /// <summary>
    ///   Begins a new <see cref="OperationScope"/>.
    /// </summary>
    /// <returns>
    ///   A new <see cref="OperationScope"/>.  The caller is responsible for
    ///   disposing the scope.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public OperationScope Begin()
    {
        return new(_logger, _logLevel, _name);
    }

    #region Do/DoAsync

    // The abstractions required to de-duplicate these methods are either not
    // available in all target frameworks or result in exceedingly obtuse code.

    /// <summary>
    ///   Performs the specified operation in an <see cref="OperationScope"/>,
    ///   capturing any exception.
    /// </summary>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Do(Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            action();
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified operation in an <see cref="OperationScope"/>,
    ///   capturing any exception.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of <paramref name="arg"/>.
    /// </typeparam>
    /// <param name="arg">
    ///   An argument to pass to <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Do<T>(T arg, Action<T> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            action(arg);
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified operation in an <see cref="OperationScope"/>,
    ///   capturing any exception.
    /// </summary>
    /// <typeparam name="TResult">
    ///   The type of result produced by <paramref name="action"/>.
    /// </typeparam>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   The result of <paramref name="action"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Do<TResult>(Func<TResult> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            return action();
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified operation in an <see cref="OperationScope"/>,
    ///   capturing any exception.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of <paramref name="arg"/>.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///   The type of result produced by <paramref name="action"/>.
    /// </typeparam>
    /// <param name="arg">
    ///   An argument to pass to <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   The result of <paramref name="action"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Do<T, TResult>(T arg, Func<T, TResult> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            return action(arg);
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified asynchronous operation in an
    ///   <see cref="OperationScope"/>, capturing any exception.
    /// </summary>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task DoAsync(Func<Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            await action();
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified asynchronous operation in an
    ///   <see cref="OperationScope"/>, capturing any exception.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of <paramref name="arg"/>.
    /// </typeparam>
    /// <param name="arg">
    ///   An argument to pass to <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task DoAsync<T>(T arg, Func<T, Task> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            await action(arg);
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified asynchronous operation in an
    ///   <see cref="OperationScope"/>, capturing any exception.
    /// </summary>
    /// <typeparam name="TResult">
    ///   The type of result produced by <paramref name="action"/>.
    /// </typeparam>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   A task representing the asynchronous operation.  When the task
    ///   completes, its <see cref="Task{TResult}.Result"/> property is set to
    ///   the result of <paramref name="action"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResult> DoAsync<TResult>(Func<Task<TResult>> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            return await action();
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    /// <summary>
    ///   Performs the specified asynchronous operation in an
    ///   <see cref="OperationScope"/>, capturing any exception.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of <paramref name="arg"/>.
    /// </typeparam>
    /// <typeparam name="TResult">
    ///   The type of result produced by <paramref name="action"/>.
    /// </typeparam>
    /// <param name="arg">
    ///   An argument to pass to <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    ///   The operation to perform.
    /// </param>
    /// <returns>
    ///   A task representing the asynchronous operation.  When the task
    ///   completes, its <see cref="Task{TResult}.Result"/> property is set to
    ///   the result of <paramref name="action"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResult> DoAsync<T, TResult>(T arg, Func<T, Task<TResult>> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        using var scope = Begin();

        try
        {
            return await action(arg);
        }
        catch (Exception e)
        {
            scope.Exception = e;
            throw;
        }
    }

    #endregion
}
