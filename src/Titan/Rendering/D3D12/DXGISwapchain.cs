using Titan.Application.Services;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;
using Titan.Windows.Win32;

namespace Titan.Rendering.D3D12;
internal sealed unsafe class DXGISwapchain : IService
{
    private const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    private ComPtr<IDXGISwapChain3> _swapchain;
    public bool Init(D3D12CommandQueue commandQueue, IWindow window, bool debug)
    {
        
        var flags = debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;
        using ComPtr<IDXGIFactory7> factory = default;
        var hr = DXGICommon.CreateDXGIFactory2(flags, factory.UUID, (void**)factory.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to create {nameof(IDXGIFactory7)}. HRESULT = {hr}");
            return false;
        }

        DXGI_SWAP_CHAIN_DESC1 desc = new()
        {
            BufferCount = BufferCount,
            AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_UNSPECIFIED,
            BufferUsage = DXGI_USAGE.DXGI_CPU_ACCESS_NONE,
            Flags = DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING,
            Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
            Height = window.Height,
            Width = window.Width,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Scaling = DXGI_SCALING.DXGI_SCALING_NONE,
            Stereo = false,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD
        };

        Logger.Trace<DXGISwapchain>($"Creating Swapchain for HWND. Width = {window.Width} Height = {window.Height} BufferCount = {BufferCount}");
        // command queue needed!
        hr = factory.Get()->CreateSwapChainForHwnd((IUnknown*)commandQueue.CommandQueue, window.NativeHandle, &desc, null, null, (IDXGISwapChain1**)_swapchain.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to create {nameof(IDXGISwapChain3)}. HRESULT = {hr}");
            return false;
        }

        return true;
    }


    public void Shutdown()
    {
        _swapchain.Dispose();
    }
}
//[InlineArray((int)GlobalConfiguration.MaxRenderFrames)]
//internal struct Backbuffers
//{
//    //public D3D12Texture2D Backbuffer;
//    public void ReleaseResources()
//    {
//        foreach (ref var backbuffer in this)
//        {
//            backbuffer.Resource.Dispose();
//        }
//    }

//    //public void Destroy(D3D12Allocator allocator)
//    //{
//    //    foreach (ref var backbuffer in this)
//    //    {
//    //        backbuffer.Resource.Dispose();
//    //        allocator.Free(backbuffer.RTV);
//    //        backbuffer = default;
//    //    }
//    //}
//}
