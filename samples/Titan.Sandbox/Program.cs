using Titan;
using Titan.Application;
using Titan.Assets;
using Titan.Core.Logging;
using Titan.Graphics.Rendering;
using Titan.Graphics.Resources;
using Titan.Input;
using Titan.Sandbox;
using Titan.Systems;
using Titan.Windows;

using var _ = Logger.Start<ConsoleLogger>(10_000);

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
    .AddRegistry<SandboxRegistry>()
    .Build()
    .Run();

namespace Titan.Sandbox
{
    internal class GameModule : IModule
    {
        public static bool Build(IAppBuilder builder, AppConfig config)
        {
            builder.AddSystems<ATestSystem>();
            return true;
        }
    }


    internal partial struct ATestSystem
    {
        [System(SystemStage.Update, SystemExecutionType.Inline)]
        public static void Update(in InputState inputState)
        {
            if (inputState.IsKeyDown(KeyCode.W))
            {
                Logger.Info<ATestSystem>("Moving forward!");
            }

            if (inputState.IsKeyDown(KeyCode.A))
            {
                Logger.Info<ATestSystem>("Turning left");
            }

            if (inputState.IsKeyDown(KeyCode.D))
            {
                Logger.Info<ATestSystem>("Turning right");
            }

            if (inputState.IsKeyDown(KeyCode.S))
            {
                Logger.Info<ATestSystem>("Moving backwards");
            }
        }


        private static AssetHandle<MeshAsset> _assetHandle;
        [System]
        public static void LoadModelTest(IAssetsManager assetsManager)
        {
            if (_assetHandle.IsInvalid)
            {
                _assetHandle = assetsManager.Load<MeshAsset>(SandboxRegistry.TileLowRed);;
            }
        }
    }
}
