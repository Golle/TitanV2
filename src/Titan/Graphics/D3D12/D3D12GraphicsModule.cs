using Titan.Application;
using Titan.Graphics.D3D12.Adapters;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Upload;
using Titan.Graphics.D3D12.Utils;

namespace Titan.Graphics.D3D12;

internal sealed class D3D12GraphicsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
#if DEBUG
            .AddSystems<D3D12DebugLayer>()
#endif
            .AddSystemsAndResource<D3D12Adapter>()
            .AddSystemsAndResource<D3D12Device>()
            .AddSystemsAndResource<D3D12CommandQueue>()
            .AddSystemsAndResource<D3D12Allocator>()
            .AddSystemsAndResource<D3D12UploadQueue>()
            .AddSystemsAndResource<D3D12ResourceManager>()

            .AddSystemsAndResource<DXGISwapchain>()
            ;

        return true;
    }
}
