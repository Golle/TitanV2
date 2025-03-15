using Titan.Application;
using Titan.Rendering.RenderPasses;

namespace Titan.Rendering;

internal sealed class DeferredRenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (config.BuiltInRendererFlags.HasFlag(BuiltInRendererFlags.GBuffer))
        {
            builder.AddSystemsAndResource<GBufferRenderPass>();
        }

        if (config.BuiltInRendererFlags.HasFlag(BuiltInRendererFlags.AmbientOcclusion))
        {
            builder.AddSystemsAndResource<GroundTruthAmbientOcclusionPass>();
        }

        if (config.BuiltInRendererFlags.HasFlag(BuiltInRendererFlags.DeferredLighting))
        {
            builder.AddSystemsAndResource<DeferredLightingRenderPass>();
        }

        

        return true;
    }
}
