// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

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
