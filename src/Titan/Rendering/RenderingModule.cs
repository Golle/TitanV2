using Titan.Application;
using Titan.Graphics.D3D12;
using Titan.Graphics.Vulkan;
using Titan.Rendering.RenderPasses;
using Titan.Rendering.Resources;

namespace Titan.Rendering;

internal sealed class RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddModule<ResourcesModule>()

            .AddSystemsAndResource<RenderGraph>()
            .AddSystemsAndResource<RenderTargetCache>()

            .AddModule<DeferredRenderingModule>()
            .AddModule<UIRenderingModule>()

            .AddSystemsAndResource<BackbufferRenderPass>()
            ;

        if (config.BuiltInRendererFlags.HasFlag(BuiltInRendererFlags.DebugRenderer))
        {
            builder.AddSystemsAndResource<DebugRenderPass>();
        }

        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder.AddModule<D3D12GraphicsModule>();
        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<VulkanModule>();
        }

        return true;
    }
}
