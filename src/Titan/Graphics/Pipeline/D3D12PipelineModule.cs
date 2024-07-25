using Titan.Application;
using Titan.Assets;
using Titan.Core.Ids;

namespace Titan.Graphics.Pipeline;

internal sealed class D3D12PipelineModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12PipelineStateObjectRegistry>()
            .AddSystemsAndResource<Graph.D3D12RenderGraph>()
            //.AddSystemsAndResource<D3D12RenderGraph1>()
            ;

        return true;
    }
}
