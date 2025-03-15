using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Events;
using Titan.Modules;

namespace Titan;

public enum ApplicationType
{
    Game,
    Headless
}

[Flags]
public enum BuiltInRendererFlags
{
    GBuffer = 1 << 0,
    AmbientOcclusion = 1 << 1,
    DeferredLighting = 1 << 2,
    UIRenderer = 1 << 3,
    DebugRenderer = 1 << 4,

#if DEBUG
    Default = GBuffer | AmbientOcclusion | DeferredLighting | UIRenderer | DebugRenderer
#else
    Default = GBuffer | AmbientOcclusion | DeferredLighting | UIRenderer
#endif
}

public record AppConfig(string Name, string Version)
{
    public ApplicationType ApplicationType { get; init; } = ApplicationType.Game;
    public BuiltInRendererFlags BuiltInRendererFlags { get; init; } = BuiltInRendererFlags.Default;
    public MemoryConfig Memory { get; init; } = MemoryConfig.Default;
    public JobSystemConfig JobSystem { get; init; } = JobSystemConfig.Default;
    public EventConfig EventConfig { get; init; } = EventConfig.Default;

    public string? EnginePath { get; init; }
    public string? ContentPath { get; init; }
}

public static class App
{
    public static IAppBuilder Create(AppConfig config)
    {
        try
        {
            return new AppBuilder(config)
                .AddModule<CoreModule>()
                .AddModule<ApplicationModule>();
        }
        catch (Exception e)
        {
            Logger.Info($"Init failed with {e.GetType().Name}. Message = {e.Message}");
            throw;
        }
        finally
        {
            Logger.Info("Init AppBuilder completed.", typeof(App));
        }
    }
}
