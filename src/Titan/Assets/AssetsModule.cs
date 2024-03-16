using Titan.Application;

namespace Titan.Assets;
internal sealed class AssetsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<AssetsContext>()
            .AddService<IAssetsManager, AssetsManager>(new AssetsManager())
            .AddSystems<AssetSystem>()
            .AddRegistry<EngineAssetsRegistry>(true)
            ;

        return true;
    }
}
