using Titan.Application;
using Titan.Graphics.D3D12;
using Titan.Graphics.Pipeline;
using Titan.Graphics.Rendering.D3D12;
using Titan.Graphics.Resources;
using Titan.Graphics.Vulkan;

namespace Titan.Graphics.Rendering;

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder
                .AddModule<D3D12GraphicsModule>()
                .AddModule<D3D12PipelineModule>()
                .AddModule<D3D12RenderingModule>()
                .AddAssetLoader<ShaderLoader>()
                .AddAssetLoader<ShaderInfoLoader>()
                .AddAssetLoader<TextureLoader>()
                .AddAssetLoader<MeshLoader>()
                ;

        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<VulkanModule>();
        }

        return true;
    }
}
