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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Subatomix.Logging.Debugger;

/// <summary>
///   Extensions methods to add <see cref="DebuggerLoggerProvider"/>.
/// </summary>
public static class DebuggerLoggingBuilderExtensions
{
    /// <summary>
    ///   Adds a debugger logger to the factory.  The logger sends messages to
    ///   an attached debugger.  In Visual Studio, logged messages will appear
    ///   in the debug output window.
    /// </summary>
    /// <param name="builder">
    ///   The logging builder to use.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ILoggingBuilder AddDebugger(this ILoggingBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, DebuggerLoggerProvider>()
        );

        return builder;
    }
}
