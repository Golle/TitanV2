namespace Titan.Systems;

public enum SystemStage
{
    /// <summary>
    /// PreInit is run immediately when the System is added. This should only be used for core engine systems that needs to be set up in a specific order.
    /// </summary>
    PreInit,
    /// <summary>
    /// Init is scheduled and run in parallel to make init phase as fast as possible.
    /// <remarks>Init is only run once at startup before the main loop starts</remarks>
    /// </summary>
    Init,

    First,
    PreUpdate,
    Update,
    PostUpdate,
    Last,

    /// <summary>
    /// Shutdown is run in parallel, same scheduling mechanings as the main loop
    /// </summary>
    Shutdown,
    /// <summary>
    /// Post shutdown is intended for core engine systems like threads, memory and IO.
    /// </summary>
    PostShutdown,

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
