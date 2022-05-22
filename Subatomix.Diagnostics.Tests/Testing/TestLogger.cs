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

using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics.Testing;

/// <summary>
///   An <see cref="ILogger"/> implementation for testing.
/// </summary>
internal class TestLogger : ILogger
{
    /// List of entries written using the logger
    private readonly List<(LogLevel, string, Exception?)>
        _entries = new();

    /// Stack of scopes issued by the logger
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
    public IReadOnlyList<(LogLevel LogLevel, string Message, Exception? Exception)>
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
    public IDisposable BeginScope<TState>(TState state)
        => new Scope<TState>(this, state);

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel                         logLevel,
        EventId                          eventId,
        TState                           state,
        Exception?                       exception,
        Func<TState, Exception?, string> formatter)
        => _entries.Add((logLevel, formatter(state, exception), exception));

    /// <summary>
    ///   A logical operation scope created by <see cref="BeginScope"/>.
    /// </summary>
    public abstract class Scope : IDisposable
    {
        private readonly Stack<Scope> _scopes;

        private protected Scope(TestLogger logger)
        {
            _scopes = logger._scopes;
            _scopes.Push(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _scopes.Count.Should().BePositive();
            _scopes.Peek().Should().BeSameAs(this);
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
        internal Scope(TestLogger logger, TState state)
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
