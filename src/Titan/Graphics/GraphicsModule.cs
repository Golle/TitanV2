using Titan.Application;
using Titan.Graphics.Rendering;
using Titan.Graphics.Rendering.D3D12;

namespace Titan.Graphics;
internal class GraphicsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddModule<RenderingModule>()
            .AddSystemsAndResource<RenderContext>()
            ;

        return true;
    }
}
