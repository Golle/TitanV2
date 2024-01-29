using Titan;
using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var appConfig = new AppConfig("Titan.Sandbox", "0.0.1");

App.Create(appConfig)
    .AddModule<GameModule>()
    .AddConfig(new MemoryConfig(10, 10))
    .AddConfig(new WindowConfig(1024, 768, true, true))
    .BuildAndRun();

internal class GameModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {

        return true;
    }

    public static bool Init(IApp app)
    {
        return true;
    }

    public static bool Shutdown(IApp app)
    {
        return true;
    }
}
