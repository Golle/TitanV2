using Titan.Application;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Modules;

namespace Titan;

public record AppConfig(string Name, string Version)
{
    public MemoryConfig Memory { get; init; } = MemoryConfig.Default;
    public JobSystemConfig JobSystem { get; init; } = JobSystemConfig.Default;
}

public static class App
{
    public static IAppBuilder Create(AppConfig config) =>
        new AppBuilder(config)
            .AddModule<CoreModule>();
}
