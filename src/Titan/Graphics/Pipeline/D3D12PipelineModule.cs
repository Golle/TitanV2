using Titan.Application;

namespace Titan.Graphics.Pipeline;

internal sealed class D3D12PipelineModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12PipelineStateObjectRegistry>();

        return true;
    }
}
