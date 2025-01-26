using Titan.Application;
using Titan.Rendering.RenderPasses;

namespace Titan.Rendering;

internal sealed class UIRenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (config.BuiltInRendererFlags.HasFlag(BuiltInRendererFlags.UIRenderer))
        {
            builder.AddSystemsAndResource<UIRenderPass>();
        }

        return true;
    }
}
