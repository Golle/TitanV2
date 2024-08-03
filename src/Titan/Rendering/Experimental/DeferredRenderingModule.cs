using Titan.Application;
using Titan.Rendering.Experimental.RenderPasses;

namespace Titan.Rendering.Experimental;

internal class DeferredRenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<RenderTargetCache>()
            .AddSystemsAndResource<GBufferRenderPass>()
            .AddSystemsAndResource<DeferredLightingRenderPass>()
            .AddSystemsAndResource<BackbufferRenderPass>()
            .AddSystemsAndResource<RenderGraph>();
        return true;
    }
}
