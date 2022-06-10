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

using Microsoft.Extensions.Options;

namespace Subatomix.Logging.Testing;

/// <summary>
///   An implementation of <see cref="IOptionsMonitor{TOptions}"/> for testing.
/// </summary>
/// <typeparam name="TOptions">
///   The options type.
/// </typeparam>
internal class TestOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    where TOptions : new()
{
    // Mock repository used to create verifiable IDisposable mocks
    private readonly MockRepository _mocks;

    // Collection of options change listeners
    private readonly List<Action<TOptions, string>> _listeners = new();

    /// <summary>
    ///   Initializes a new <see cref="TestOptionsMonitor{TOptions}"/> instance
    ///   using the specified mock repository.
    /// </summary>
    /// <param name="mocks">
    ///   The mock repository to use to create mocks.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="mocks"/> is <see langword="null"/>.
    /// </exception>
    public TestOptionsMonitor(MockRepository mocks)
    {
        _mocks = mocks ?? throw new ArgumentNullException(nameof(mocks));
    }

    /// <summary>
    ///   Gets the dictionary of named <typeparamref name="TOptions"/> values.
    /// </summary>
    public Dictionary<string, TOptions> Values { get; } = new();

    /// <summary>
    ///   Gets or sets the current <typeparamref name="TOptions"/> instance
    ///   with the name <see cref="Options.DefaultName"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///   Attempted to set this property to <see langword="null"/>.
    /// </exception>
    public TOptions CurrentValue
    {
        get => Get(Options.DefaultName);
        set => Set(Options.DefaultName, value);
    }

    /// <inheritdoc/>
    public TOptions Get(string name)
    {
        return Values.TryGetValue(name, out var value)
            ? value
            : Values[name] = new();
    }

    private void Set(string name, TOptions value)
    {
        Values[name] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc/>
    IDisposable IOptionsMonitor<TOptions>.OnChange(Action<TOptions, string> listener)
    {
        _listeners.Add(listener);

        var token = _mocks.Create<IDisposable>();

        token
            .Setup(t => t.Dispose())
            .Callback(() => _listeners.Remove(listener))
            .Verifiable();

        return token.Object;
    }

    /// <summary>
    ///   Notifies listeners that the <typeparamref name="TOptions"/> instance
    ///   with the specified name has changed.
    /// </summary>
    /// <param name="name">
    ///   The name of the <see cref="TOptions"/> instance.
    /// </param>
    public void NotifyChanged(string? name = null)
    {
        var options = Get(name ??= Options.DefaultName);

        _listeners.ForEach(a => a(options, name));
    }
}
