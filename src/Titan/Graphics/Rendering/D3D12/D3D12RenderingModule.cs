using Titan.Application;
using Titan.Graphics.Resources;

namespace Titan.Graphics.Rendering.D3D12;
internal class D3D12RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12FullScreenRenderer>()
            .AddAssetLoader<ShaderLoader>()
            .AddAssetLoader<TextureLoader>()
            ;

        return true;
    }
}
