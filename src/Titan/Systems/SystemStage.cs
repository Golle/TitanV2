namespace Titan.Systems;

public enum SystemStage
{
    First,
    PreUpdate,
    Update,
    PostUpdate,
    Last,
    Count
}

public enum SystemExecutionType
{
    /// <summary>
    /// The System will be scheduled and run in parallel
    /// </summary>
    Normal,
    /// <summary>
    /// Before the system is scheduled, a check if the system should be run is done (NYI)
    /// </summary>
    Check,
    /// <summary>
    /// The system will not be scheduled but executed as soon as its dependencies have been completed. This will run on the scheduler thread, so make sure the system is almost a no-op call.
    /// </summary>
    Inline,
    /// <summary>
    /// Same as inline but the Check function will be invoked first.
    /// </summary>
    InlineCheck
}
