using Titan.Application;
using Titan.Rendering.D3D12New.Adapters;
using Titan.Rendering.D3D12New.Memory;

namespace Titan.Rendering.D3D12New;

internal sealed class D3D12Module : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12Adapter>()
            .AddSystemsAndResource<D3D12Device>()
            .AddSystemsAndResource<D3D12CommandQueue>()
            .AddSystemsAndResource<D3D12Allocator>()
            .AddResource<D3D12DescriptorHeaps>()

            .AddSystemsAndResource<DXGISwapchain>()
            ;

        return true;
    }
}
