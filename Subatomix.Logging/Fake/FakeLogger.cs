// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging.Fake;

/// <summary>
///   An <see cref="ILogger"/> implementation that captures logged entries into
///   a list.
/// </summary>
public class FakeLogger : ILogger
{
    // List of entries written using the logger
    private readonly List<(LogLevel, EventId, string, Exception?)>
        _entries = new();

    // Stack of scopes issued by the logger
    private readonly Stack<Scope>
        _scopes = new();

    /// <summary>
    ///   Gets the collection of entries written via the logger.
    /// </summary>
    /// <remarks>
    ///   ℹ <strong>Note:</strong>
    ///   This collection contains all entries written via the logger,
    ///   regardless of the minimum severity level set by the
    ///   <see cref="MinimumLevel"/> property.
    /// </remarks>
    public IReadOnlyList<(LogLevel LogLevel, EventId Id, string Message, Exception? Exception)>
        Entries => _entries;

    /// <summary>
    ///   Gets the stack of logical operation scopes issued by the logger.
    /// </summary>
    public IReadOnlyCollection<Scope>
        Scopes => _scopes;

    /// <summary>
    ///   Gets or sets the minimum severity level for which
    ///   <see cref="IsEnabled"/> returns <see langword="true"/>.
    /// </summary>
    /// <remarks>
    ///   ℹ <strong>Note:</strong>
    ///   This property affects the behavior of <see cref="IsEnabled"/> only.
    ///   The <see cref="Entries"/> property collects all entries written via
    ///   the logger, regardless of the minimum severity level.
    /// </remarks>
    public LogLevel MinimumLevel { get; set; }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= MinimumLevel;

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => new Scope<TState>(this, state);

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel                         logLevel,
        EventId                          eventId,
        TState                           state,
        Exception?                       exception,
        Func<TState, Exception?, string> formatter)
    {
        _entries.Add((logLevel, eventId, formatter(state, exception), exception));
    }

    /// <summary>
    ///   A logical operation scope created by <see cref="BeginScope"/>.
    /// </summary>
    public abstract class Scope : IDisposable
    {
        private readonly Stack<Scope> _scopes;

        private protected Scope(FakeLogger logger)
        {
            _scopes = logger._scopes;
            _scopes.Push(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_scopes.Count == 0 || _scopes.Peek() != this)
                throw new InvalidOperationException(
                    "Attempted to dispose a scope other than the innermost scope."
                );

            _scopes.Pop();
        }

        /// <summary>
        ///   Gets the state (identifier) associated with the scope.
        /// </summary>
        public object? State => AbstractState;

        private protected abstract object? AbstractState { get; }
    }

    /// <summary>
    ///   A logical operation scope created by <see cref="BeginScope"/>.
    /// </summary>
    /// <typeparam name="TState">
    ///   The type of state associated with the scope.
    /// </typeparam>
    public sealed class Scope<TState> : Scope
    {
        internal Scope(FakeLogger logger, TState state)
            : base(logger)
        {
            State = state;
        }

        /// <summary>
        ///   Gets the state (identifier) associated with the scope.
        /// </summary>
        public new TState State { get; }

        private protected override object? AbstractState => State;
    }
}
