using Titan.Core;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;

namespace Titan.RenderingV3;

internal struct D3D12Swapchain
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    public const DXGI_FORMAT DefaultFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

    public ComPtr<IDXGISwapChain3> Swapchain;
    public ComPtr<ID3D12Fence> Fence;

    public HANDLE FenceEvent;
    public uint BackbufferFrameIndex;

    public uint SyncInterval;
    public uint PresentFlags;

    public Inline3<ComPtr<ID3D12Resource>> Backbuffers;
    public Inline3<DescriptorHandle> RenderTargetViews;
}
