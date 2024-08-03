using Titan.Application;
using Titan.Graphics.D3D12;
using Titan.Graphics.Vulkan;
using Titan.Rendering.D3D12;
using Titan.Rendering.Resources;

namespace Titan.Rendering;

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddModule<ResourcesModule>()
            .AddModule<DeferredRenderingModule>();

        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder
                .AddModule<D3D12GraphicsModule>()
                ;
        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<VulkanModule>();
        }

        return true;
    }
}
