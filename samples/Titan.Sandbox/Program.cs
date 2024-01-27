using Titan;
using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var appConfig = new AppConfig("Titan.Sandbox");

App.Create(appConfig)
    .AddModule<GameModule>()
    .AddConfig(new MemoryConfig(10, 10))
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