namespace Titan.Systems;

public enum SystemStage
{
    /// <summary>
    /// When the engine has completed the startup phase this stage will be called, all systems marked as Startup will run synchronosly
    /// </summary>
    Startup,
    /// <summary>
    /// PreInit is executed right after Startup, using the default executor.
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

    EndOfLife,

    Count
}
