// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Subatomix.Logging;

using SD = System.Diagnostics;

/// <summary>
///   A scope representing a logical operation with an associated
///   <see cref="SD.Activity"/>.  This type is intended for use with the
///   <see cref="ILogger"/> API.
/// </summary>
/// <remarks>
///   <para>
///     On construction, a scope of this type starts a new named
///     <see cref="SD.Activity"/> and logs a start message containing the name
///     of the activity.  On disposal, the scope stops the activity and logs a
///     completion message containing the name and duration of the activity.
///   </para>
///   <para>
///     Code using this type can arrange for exception reporting by setting the
///     <see cref="Exception"/> propery, typically in a <see langword="catch"/>
///     block.  On disposal, if a scope's <see cref="Exception"/> propery is
///     not <see langword="null"/>, the scope logs the exception immediately
///     before the completion message and adds an exception indicator to the
///     completion message.  The scope also sets the activity's status if it is
///     unset:
///     if <see cref="Exception"/> is <see langword="null"/>, the scope sets
///       <see cref="Activity.Status"/> to <see cref="ActivityStatusCode.Ok"/> and
///       <see cref="Activity.StatusDescription"/> to <see langword="null"/>;
///     othwerise, the scope sets
///       <see cref="Activity.Status"/> to <see cref="ActivityStatusCode.Error"/> and 
///       <see cref="Activity.StatusDescription"/> to the exception message.
///   </para>
/// </remarks>
public class ActivityScope : OperationScope
{
    private static readonly DiagnosticListener
        DiagnosticSource = new("Subatomix.Logging.ActivityScope");

    /// <summary>
    ///   Initializes and starts a new <see cref="ActivityScope"/> instance.
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
    public ActivityScope(
        ILogger                   logger,
        LogLevel                  logLevel = LogLevel.Information,
        [CallerMemberName] string name     = null!)
    : this(logger, logLevel, name, start: true)
    { }

    /// <summary>
    ///   Initializes a new <see cref="ActivityScope"/> instance.
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
    protected ActivityScope(ILogger logger, LogLevel logLevel, string name, bool start)
        : base(logger, logLevel, name, start: false)
    {
        Activity = new(name);

        if (start) Start();
    }

    /// <summary>
    ///   Gets the activity object associated with the operation.
    /// </summary>
    public Activity Activity { get; }

    /// <inheritdoc/>
    protected override void Start()
    {
        DiagnosticSource.StartActivity(Activity, null);

        base.Start();
    }

    /// <inheritdoc/>
    protected override void Stop()
    {
        base.Stop();

        Activity.SetStatusIfUnset(Exception);
        Activity.SetTelemetryTags();

        DiagnosticSource.StopActivity(Activity, null);
    }
}
