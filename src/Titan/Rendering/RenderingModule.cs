using Titan.Application;
using Titan.Graphics.D3D12;
using Titan.Graphics.Vulkan;
using Titan.Rendering.Resources;
using Titan.Rendering.Storage;

namespace Titan.Rendering;

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddModule<StorageModule>()
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
