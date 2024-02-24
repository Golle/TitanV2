using Titan.Application;

namespace Titan.Rendering.D3D12New;

internal sealed class D3D12Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<D3D12Adapter>()
            .AddSystems<D3D12AdapterSystem>();


        return true;
    }
}
