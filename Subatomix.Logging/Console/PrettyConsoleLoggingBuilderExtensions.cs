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

namespace Subatomix.Logging.Console;

/// <summary>
///   Extensions methods to add <see cref="PrettyConsoleFormatter"/>.
/// </summary>
public static class PrettyConsoleLoggingBuilderExtensions
{
    /// <summary>
    ///   Adds a console logger using a log formatter named 'pretty' to the
    ///   factory with default options.
    /// </summary>
    /// <param name="builder">
    ///   The logging builder to use.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ILoggingBuilder AddPrettyConsole(this ILoggingBuilder builder)
    {
        return builder
            .AddPrettyConsoleFormatter()
            .AddConsole(c => c.FormatterName = PrettyConsoleFormatter.Name);
    }

    /// <summary>
    ///   Adds a console logger using a log formatter named 'pretty' to the
    ///   factory with the specified options.
    /// </summary>
    /// <param name="builder">
    ///   The logging builder to use.
    /// </param>
    /// <param name="configure">
    ///   A delegate to configure options for the log formatter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="builder"/> and/or <paramref name="configure"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public static ILoggingBuilder AddPrettyConsole(
        this ILoggingBuilder                  builder,
        Action<PrettyConsoleFormatterOptions> configure)
    {
        return builder
            .AddPrettyConsoleFormatter(configure)
            .AddConsole(c => c.FormatterName = PrettyConsoleFormatter.Name);
    }

    /// <summary>
    ///   Adds a console log formatter named 'pretty' to the factory with
    ///   default options.
    /// </summary>
    /// <param name="builder">
    ///   The logging builder to use.
    /// </param>
    /// <remarks>
    ///   <strong>NOTE:</strong> This method adds the the formatter only.
    ///   To add the formatter and a console logger using it, use
    ///   <see cref="AddPrettyConsole(ILoggingBuilder)"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static ILoggingBuilder AddPrettyConsoleFormatter(this ILoggingBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddConsoleFormatter<
            PrettyConsoleFormatter,
            PrettyConsoleFormatterOptions
        >();
    }

    /// <summary>
    ///   Adds a console log formatter named 'pretty' to the factory with the
    ///   specified options.
    /// </summary>
    /// <param name="builder">
    ///   The logging builder to use.
    /// </param>
    /// <param name="configure">
    ///   A delegate to configure options for the log formatter.
    /// </param>
    /// <remarks>
    ///   <strong>NOTE:</strong> This method adds the the formatter only.
    ///   To add the formatter and a console logger using it, use
    ///   <see cref="AddPrettyConsole(ILoggingBuilder, Action{PrettyConsoleFormatterOptions})"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="builder"/> and/or <paramref name="configure"/> is
    ///   <see langword="null"/>.
    /// </exception>
    public static ILoggingBuilder AddPrettyConsoleFormatter(
        this ILoggingBuilder                  builder,
        Action<PrettyConsoleFormatterOptions> configure)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddConsoleFormatter<
            PrettyConsoleFormatter,
            PrettyConsoleFormatterOptions
        >(configure);
    }
}
