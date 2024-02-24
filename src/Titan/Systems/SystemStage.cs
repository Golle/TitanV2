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
