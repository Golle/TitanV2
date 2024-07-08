using Titan.Application;
using Titan.Graphics.Rendering;

namespace Titan.Graphics;
internal class GraphicsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddModule<RenderingModule>()
            ;

        return true;
    }
}
