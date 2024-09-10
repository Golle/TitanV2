using Titan.Application;

namespace Titan.Rendering.Resources;
internal class ResourcesModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddAssetLoader<ShaderLoader>()
            .AddAssetLoader<TextureLoader>()
            .AddAssetLoader<MeshLoader>();

        return true;
    }
}
