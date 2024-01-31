using Titan;
using Titan.Application;
using Titan.Core.Logging;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);

var appConfig = new AppConfig("Titan.Sandbox", "0.0.1")
{
    EnginePath = EngineHelper.GetEngineFolder("Titan.sln"),
    ContentPath = EngineHelper.GetContentPath("Titan.Sandbox.csproj", "Assets")
};

App.Create(appConfig)
    .AddModule<GameModule>()
    .AddConfig(new WindowConfig(1024, 768, true, true))
    //.AddConfig(RenderingConfig.Default with
    //{
    //    Adapter = new AdapterConfig(8712, 4318)
    //})
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
