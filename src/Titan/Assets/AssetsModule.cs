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
            .AddSystems<AssetLoaderSystem>()
            .AddRegistry<EngineAssetsRegistry>(true)
            ;

        return true;
    }
}
