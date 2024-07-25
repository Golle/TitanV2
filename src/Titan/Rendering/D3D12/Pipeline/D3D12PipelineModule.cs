using Titan.Application;

namespace Titan.Rendering.D3D12.Pipeline;

internal sealed class D3D12PipelineModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12PipelineStateObjectRegistry>()
            .AddSystemsAndResource<D3D12RenderGraph>()
            //.AddSystemsAndResource<D3D12RenderGraph1>()
            ;

        return true;
    }
}
