using Titan.Application;
using Titan.Assets.HotReload;

namespace Titan.Assets;
internal sealed class AssetsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<AssetSystem>()

#if HOT_RELOAD_ASSETS
            .AddSystemsAndResource<AssetFileWatcher>()
#endif
            .AddRegistry<EngineAssetsRegistry>(true)
            ;

        return true;
    }
}
