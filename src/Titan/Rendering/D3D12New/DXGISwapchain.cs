using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12New;

[UnmanagedResource]
internal unsafe partial struct DXGISwapchain
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    public const DXGI_FORMAT DefaultFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

    public ComPtr<IDXGISwapChain3> Swapchain;
    public ComPtr<ID3D12Fence> Fence;
    public Inline3<D3D12Texture2D> Backbuffers;

    public HANDLE FenceEvent;
    public uint FrameIndex;

    public ulong GPUFrame;
    public ulong CPUFrame;


    [System(SystemStage.Init)]
    public static void Init(DXGISwapchain* swapchain, in Window window, in D3D12CommandQueue commandQueue, in Memory.D3D12Allocator allocator, in D3D12Device device, IConfigurationManager configurationManager)
    {
        Debug.Assert(BufferCount <= 3, "The Backbuffers are stored in Inline3 struct, change this if we want to run more than 3 backbuffers.");
        var config = configurationManager.GetConfigOrDefault<RenderingConfig>();
        var flags = config.Debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;

        using ComPtr<IDXGIFactory7> factory = default;
        var hr = DXGICommon.CreateDXGIFactory2(flags, IDXGIFactory7.Guid, (void**)factory.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to create the {nameof(IDXGIFactory7)}. HRESULT = {hr}");
            return;
        }

        DXGI_SWAP_CHAIN_DESC1 desc = new()
        {
            BufferCount = BufferCount,
            AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_UNSPECIFIED,
            BufferUsage = DXGI_USAGE.DXGI_CPU_ACCESS_NONE,
            Flags = DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING, //TODO(Jens): This should be configurable
            Format = DefaultFormat,
            Height = (uint)window.Height,
            Width = (uint)window.Width,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Scaling = DXGI_SCALING.DXGI_SCALING_NONE,
            Stereo = false,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD
        };
        hr = factory.Get()->CreateSwapChainForHwnd((IUnknown*)commandQueue.Queue.Get(), window.Handle, &desc, null, null, (IDXGISwapChain1**)swapchain->Swapchain.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Falied to create the {nameof(IDXGISwapChain3)}. HRESULT = {hr}");
            return;
        }

        swapchain->Fence = device.CreateFence();
        if (!swapchain->Fence.IsValid)
        {
            Logger.Error<IDXGISwapChain3>("Failed to create the Fence");
            return;
        }

        fixed (char* pName = $"{nameof(DXGISwapchain)}_FenceEvent")
        {
            swapchain->FenceEvent = Kernel32.CreateEventW(null, 0, 0, pName);
        }

        // Disable Alt-enter (will be handled by windows input)
        {
            hr = factory.Get()->MakeWindowAssociation(window.Handle, DXGI_MAKE_WINDOW_ASSOCIATION_FLAGS.DXGI_MWA_NO_ALT_ENTER);
            if (FAILED(hr))
            {
                Logger.Error<DXGISwapchain>($"Failed to disable Alt+Enter. HRESULT = {hr}");
            }
        }

        swapchain->FrameIndex = swapchain->Swapchain.Get()->GetCurrentBackBufferIndex();


    }

    [System]
    public static void Update(in DXGISwapchain swapchain)
    {


    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown(DXGISwapchain* swapchain)
    {
        swapchain->FlushGPU();

        // Flush GPU

        swapchain->Fence.Dispose();
        swapchain->Swapchain.Dispose();
        *swapchain = default;

    }

    private void FlushGPU()
    {
        // Flush the GPU
    }

}
