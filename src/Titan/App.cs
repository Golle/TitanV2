using Titan.Application;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Modules;

namespace Titan;


public enum ApplicationType
{
    Game,
    Headless
}

public record AppConfig(string Name, string Version)
{
    public ApplicationType ApplicationType { get; init; } = ApplicationType.Game;
    public MemoryConfig Memory { get; init; } = MemoryConfig.Default;
    public JobSystemConfig JobSystem { get; init; } = JobSystemConfig.Default;

    public string? EnginePath { get; init; }
    public string? ContentPath { get; init; }
}

public static class App
{
    public static IAppBuilder Create(AppConfig config)
    {
        return new AppBuilder(config)
            .AddModule<CoreModule>()
            .AddModule<ApplicationModule>();
    }
}
