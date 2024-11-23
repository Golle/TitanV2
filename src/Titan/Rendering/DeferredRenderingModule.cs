using Titan.Application;
using Titan.Rendering.RenderPasses;

namespace Titan.Rendering;

internal sealed class DeferredRenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<GBufferRenderPass>()
            .AddSystemsAndResource<DeferredLightingRenderPass>()
            .AddSystemsAndResource<BackbufferRenderPass>()
            .AddSystemsAndResource<UIRenderPass>()
            ;
        return true;
    }
}
