using System.Diagnostics;

namespace Subatomix.Diagnostics.Legacy;

/// <summary>
///   A listener to flow <see cref="Activity"/> start and stop events to the
///   legacy <see cref="CorrelationManager"/> facility.
/// </summary>
/// <remarks>
///   <para>
///     An instance of this class listens for <see cref="Activity"/> start and
///     stop events, filtered by predicate methods.
///   </para>
///   <para>
///     When an activity starts and is not excluded by predicate, the listener
///       pushes <see cref="Activity.Id"/> onto the
///       <see cref="CorrelationManager.LogicalOperationStack"/>.
///     If the stack was empty, the listener also sets 
///       <see cref="CorrelationManager.ActivityId"/> to a <see cref="Guid"/>
///       derived deterministically from <see cref="Activity.Id"/>.
///   </para>
///   <para>
///     When an activity stops that was not excluded previously by predicate,
///     the listener pops the current item from the
///       <see cref="CorrelationManager.LogicalOperationStack"/>.
///     If the stack becomes empty, the listener also sets
///       <see cref="CorrelationManager.ActivityId"/> to
///       <see cref="Guid.Empty"/>.
///   </para>
///   <para>
///     Invoke the <see cref="Register"/> method to begin listening for
///     activity start and stop events.  Dispose the listener to unregister.
///     To limit which activity start and stop events flow to the correlation
///     manager, override the <see cref="ShouldFlow(ActivitySource)"/> and/or
///     <see cref="ShouldFlow(Activity)"/> methods.
///   </para>
/// </remarks>
public class CorrelationManagerActivityListener : IDisposable
{
    private const string
        MarkerName  = "Subatomix:FlowToCorrelationManager",
        MarkerValue = "";

    private readonly ActivityListener _listener;

    /// <summary>
    ///   Initializes a new <see cref="CorrelationManagerActivityListener"/>
    ///   instance.
    /// </summary>
    /// <remarks>
    ///   Invoke the <see cref="Register"/> method to begin listening for
    ///   activity start and stop events.  Dispose the listener to unregister.
    /// </remarks>
    public CorrelationManagerActivityListener()
    {
        _listener = new()
        {
            ActivityStarted = OnActivityStarted,
            ActivityStopped = OnActivityStopped,
            ShouldListenTo  = ShouldFlow,
        };
    }

    /// <summary>
    ///   Begins listening for <see cref="Activity"/> start and stop events.
    /// </summary>
    public void Register()
    {
        ActivitySource.AddActivityListener(_listener);
    }

    /// <summary>
    ///   Determines whether activities from the specified source should flow
    ///   to the <see cref="CorrelationManager"/>.
    /// </summary>
    /// <param name="source">
    ///   The activity source to check.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if activities from <paramref name="source"/>
    ///     should flow to the <see cref="CorrelationManager"/>;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Derived classes can override this method to exclude unwanted activity
    ///   sources.
    /// </remarks>
    protected virtual bool ShouldFlow(ActivitySource source)
    {
        return true;
    }

    /// <summary>
    ///   Determines whether the specified activity should flow to
    ///   <see cref="CorrelationManager"/>.
    /// </summary>
    /// <param name="activity">
    ///   The activity to check.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if <paramref name="activity"/> should flow to
    ///     the <see cref="CorrelationManager"/>;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Derived classes can override this method to exclude unwanted
    ///   activities.
    /// </remarks>
    protected virtual bool ShouldFlow(Activity activity)
    {
        return true;
    }

    private void OnActivityStarted(Activity activity)
    {
        if (!ShouldFlow(activity))
            return;

        var manager = CorrelationManagerSafe;
        var stack   = manager.LogicalOperationStack;

        if (stack.Count == 0)
            manager.ActivityId = activity.GetRootOperationGuid();

        stack.Push(activity.Id);

        activity.SetCustomProperty(MarkerName, MarkerValue);
    }

    private void OnActivityStopped(Activity activity)
    {
        if (activity.GetCustomProperty(MarkerName) is null)
            return;

        var manager = CorrelationManagerSafe;
        var stack   = manager.LogicalOperationStack;

        stack.Pop();

        if (stack.Count == 0)
            manager.ActivityId = Guid.Empty;
    }

    /// <summary>
    ///   Stops listening for <see cref="Activity"/> start and stop events.
    ///   Disposes managed and unmanaged resources owned by the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(managed: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Disposes resources owned by the object.
    /// </summary>
    /// <param name="managed">
    ///   Whether to dispose managed resources
    ///   (in addition to unmanaged resources, which are always disposed).
    /// </param>
    protected virtual void Dispose(bool managed)
    {
        if (managed)
            _listener.Dispose();
    }

#if NETFRAMEWORK
    private static CorrelationManager CorrelationManagerSafe
        => Trace.CorrelationManager;

#else
    // Workaround for https://github.com/dotnet/runtime/issues/50480
    // Technique from https://github.com/dotnet/runtime/pull/62640

    private static CorrelationManager? _correlationManager;

    private static CorrelationManager CorrelationManagerSafe
        => Volatile.Read(ref _correlationManager)
        ?? Interlocked.CompareExchange(ref _correlationManager, Trace.CorrelationManager, null)
        ?? _correlationManager;
#endif
}
