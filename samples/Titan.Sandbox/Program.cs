using Titan;
using Titan.Application;
using Titan.Core.Logging;using Titan.ECS;
using Titan.Rendering;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var entity = new Entity(10, 45);

var appConfig = new AppConfig("Titan.Sandbox", "0.0.1")
{
    EnginePath = EngineHelper.GetEngineFolder("Titan.sln"),
    ContentPath = EngineHelper.GetContentPath("Titan.Sandbox.csproj", "Assets")
};

App.Create(appConfig)
    .AddModule<GameModule>()
    .AddPersistedConfig(new WindowConfig(1024, 768, true, true))
    .AddPersistedConfig(new RenderingConfig
    {
        Debug = true
    })
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
