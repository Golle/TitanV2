using Titan.Application;
using Titan.Rendering.D3D12.Renderers;

namespace Titan.Rendering.D3D12;
internal class D3D12RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12FullScreenRenderer>()
            //.AddSystemsAndResource<D3D12TextRenderer>()

            .AddSystemsAndResource<SceneRenderer>()
            .AddSystemsAndResource<DeferredLightingRenderer>()
            .AddSystemsAndResource<FinalRenderer>()
            
            ;

        return true;
    }
}
