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

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Subatomix.Diagnostics;

using SD = System.Diagnostics;

/// <summary>
///   A scope representing a correlated logical operation, intended for use
///   with the <see cref="ILogger"/>, <see cref="SD.Activity"/>, and legacy
///   <see cref="Trace.CorrelationManager"/> APIs.
/// </summary>
/// <remarks>
///   <para>
///     On construction, a scope of this type logs a start message containing
///     the name of the operation.  On disposal, the scope logs a completion
///     message containing the name and duration of the operation.
///   </para>
///   <para>
///     Code using this type can arrange for exception logging by setting the
///     <see cref="Exception"/> propery, typically in a <see langword="catch"/>
///     block.  On disposal, if a scope's <see cref="Exception"/> propery is
///     not <see langword="null"/>, the scope logs the exception immediately
///     before the completion message and adds an exception indicator to the
///     completion message.
///   </para>
///   <para>
///     Scopes of this type participate in logical operation correlation.  On
///     construction, a scope starts a new named <see cref="SD.Activity"/> and
///     a corresponding <see cref="Trace.CorrelationManager"/> operation.  On
///     disposal, the scope stops the activity and operation.  Before stopping
///     the activity, the scope sets the <see cref="Activity.Status"/> property
///     if it is unset:
///     if <see cref="Exception"/> is <see langword="null"/>, the scope sets
///         <see cref="Activity.Status"/> to <see cref="ActivityStatusCode.Ok"/> and
///         <see cref="Activity.StatusDescription"/> to <see langword="null"/>;
///     othwerise, the scope sets
///         <see cref="Activity.Status"/> to <see cref="ActivityStatusCode.Error"/> and 
///         <see cref="Activity.StatusDescription"/> to the exception message.
///   </para>
/// </remarks>
public class CorrelationScope : OperationScope
{
    /// <summary>
    ///   Initializes and starts a new <see cref="CorrelationScope"/> instance.
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
    public CorrelationScope(
        ILogger                   logger,
        LogLevel                  logLevel = LogLevel.Information,
        [CallerMemberName] string name     = null!)
    : this(logger, logLevel, name, start: true)
    { }

    /// <summary>
    ///   Initializes a new <see cref="CorrelationScope"/> instance.
    /// </summary>
    /// <param name="logger">
    ///   The logger for operation-related messages.
    /// </param>
    /// <param name="logLevel">
    ///   The severity level for operation start and completion messages.
    /// </param>
    /// <param name="name">
    ///   The name of the operation.
    /// </param>
    /// <param name="start">
    ///   <para>
    ///     Whether to invoke <see cref="OperationScope.Start"/>.
    ///   </para>
    ///   <para>
    ///     A constructor of a derived type can pass <see langword="false"/> to
    ///     permit further initialization before the logical operation starts.
    ///     The derived type constructor must then invoke
    ///     <see cref="OperationScope.Start"/> before returning.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="logger"/> and/or <paramref name="name"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="name"/> is empty.
    /// </exception>
    protected CorrelationScope(ILogger logger, LogLevel logLevel, string name, bool start)
        : base(logger, logLevel, name, start: false)
    {
        Activity = new(name);

        if (start) Start();
    }

    /// <summary>
    ///   Gets the activity object that represents the operation.
    /// </summary>
    public Activity Activity { get; }

    /// <inheritdoc/>
    protected override void Start()
    {
        Activity.Start();

        base.Start();
    }

    /// <inheritdoc/>
    protected override void Stop()
    {
        base.Stop();

        Activity.SetStatusIfUnset(Exception);
        Activity.Stop();
    }
}
