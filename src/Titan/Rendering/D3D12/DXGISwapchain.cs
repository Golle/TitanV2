using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Threading;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering.D3D12.Memory;
using Titan.Rendering.D3D12.Utils;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows.Win32;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12;

[InlineArray((int)GlobalConfiguration.MaxRenderFrames)]
internal struct Backbuffers
{
    private D3D12Texture2D _;
    public void ReleaseResources()
    {
        foreach (ref var backbuffer in this)
        {
            backbuffer.Resource.Dispose();
            backbuffer = default;
        }
    }

    //public void Destroy(D3D12Allocator allocator)
    //{
    //    foreach (ref var backbuffer in this)
    //    {
    //        backbuffer.Resource.Dispose();
    //        allocator.Free(backbuffer.RTV);
    //        backbuffer = default;
    //    }
    //}
}

[UnmanagedResource]
internal partial struct D3D12SwapchainInfo
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    public ComPtr<ID3D12Fence> Fence;
    public ComPtr<IDXGISwapChain3> Swapchain;
    public HANDLE FenceEvent;
    public ulong CpuFrame;
    public ulong GpuFrame;
    public uint FrameIndex;
    public Backbuffers Backbuffers;
    public JobHandle PresentHandle;
}

internal sealed unsafe partial class DXGISwapchain : IService
{
    private const uint BufferCount = D3D12SwapchainInfo.BufferCount;

    private UnmanagedResource<D3D12SwapchainInfo> _swapchainInfo;
    private D3D12CommandQueue? _commandQueue;
    private D3D12Device? _device;
    private D3D12Allocator? _allocator;

    public bool Init(D3D12CommandQueue commandQueue, D3D12Device device, D3D12Allocator allocator, Window* window, UnmanagedResource<D3D12SwapchainInfo> swapchainInfoHandle, bool debug)
    {
        var flags = debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;
        using ComPtr<IDXGIFactory7> factory = default;
        var hr = DXGICommon.CreateDXGIFactory2(flags, factory.UUID, (void**)factory.GetAddressOf());
        if (FAILED(hr))
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
            Height = (uint)window->Height,
            Width = (uint)window->Width,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Scaling = DXGI_SCALING.DXGI_SCALING_NONE,
            Stereo = false,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD
        };


        IDXGISwapChain3* swapchain;
        Logger.Trace<DXGISwapchain>($"Creating Swapchain for HWND. Width = {window->Width} Height = {window->Height} BufferCount = {BufferCount}");
        hr = factory.Get()->CreateSwapChainForHwnd((IUnknown*)commandQueue.CommandQueue, window->Handle, &desc, null, null, (IDXGISwapChain1**)&swapchain);
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to create {nameof(IDXGISwapChain3)}. HRESULT = {hr}");
            return false;
        }

        // Disable Alt-enter (will be handled by windows input)
        {
            hr = factory.Get()->MakeWindowAssociation(window->Handle, DXGI_MAKE_WINDOW_ASSOCIATION_FLAGS.DXGI_MWA_NO_ALT_ENTER);
            if (FAILED(hr))
            {
                Logger.Error<DXGISwapchain>($"Failed to disable Alt+Enter. HRESULT = {hr}");
            }
        }

        var fence = device.CreateFence();
        HANDLE fenceEvent;
        fixed (char* pName = $"{nameof(DXGISwapchain)}_FenceEvent")
        {
            fenceEvent = Kernel32.CreateEventW(null, 0, 0, pName);
        }

        swapchainInfoHandle.Init(new D3D12SwapchainInfo
        {
            Swapchain = swapchain,
            Fence = fence,
            FenceEvent = fenceEvent,
            CpuFrame = 0,
            GpuFrame = 0,
            FrameIndex = swapchain->GetCurrentBackBufferIndex()
        });

        _commandQueue = commandQueue;
        _device = device;
        _allocator = allocator;
        _swapchainInfo = swapchainInfoHandle;

        if (!InitBackbuffers((uint)window->Width, (uint)window->Height, true))
        {
            Logger.Error<DXGISwapchain>("Failed to init the backbuffers.");
            return false;
        }

        return true;
    }

    private bool InitBackbuffers(uint width, uint height, bool createDescriptor)
    {
        Debug.Assert(_allocator != null && _device != null);

        var info = _swapchainInfo.AsPointer;
        for (var i = 0; i < BufferCount; ++i)
        {
            ref var backbuffer = ref info->Backbuffers[i];
            var hr = info->Swapchain.Get()->GetBuffer((uint)i, ID3D12Resource.Guid, (void**)backbuffer.Resource.GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<DXGISwapchain>($"Failed to get backbuffer at index {i}. HRESULT = {hr}");
                return false;
            }

            if (createDescriptor)
            {
                backbuffer.RTV = _allocator.Allocate(DescriptorHeapType.RenderTargetView);
            }

            backbuffer.Texture2D.Width = width;
            backbuffer.Texture2D.Height = height;
            //backbuffer.Texture2D.Format = (TextureFormat)DefaultFormat;
            _device.CreateRenderTargetView(backbuffer.Resource.Get(), null, backbuffer.RTV);
            D3D12Helpers.SetName(backbuffer.Resource, $"Backbuffer_{i}");
        }
        return true;
    }


    public void Shutdown()
    {
        var info = _swapchainInfo.AsPointer;
        info->Swapchain.Dispose();
        info->Backbuffers.ReleaseResources();
        info->Fence.Dispose();
        Kernel32.CloseHandle(info->FenceEvent);
        *info = default;
    }


    [System(SystemStage.Last, SystemExecutionType.Inline)]
    public static void Present(IJobSystem jobSystem, D3D12SwapchainInfo* info)
    {
        //while (!jobSystem.IsCompleted(info->PresentHandle))
        //{
        //    // wait.. TODO: implement a proper wait function with signaling. This will consume the entire CPU core.

        //    //Thread.Yield(); // might crash :d
        //}
        //jobSystem.Reset(ref info->PresentHandle);
        //info->PresentHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&AsyncPresent, info, false));
        info->FrameIndex = (info->FrameIndex + 1) % BufferCount;

        static void AsyncPresent(D3D12SwapchainInfo* context)
        {
            //context->Swapchain.Get()->Present(1, 0);
        }
    }
}
