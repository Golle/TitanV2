using System.Diagnostics;
using Titan.Application;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Events;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Utils;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Graphics.D3D12;

[UnmanagedResource]
internal unsafe partial struct DXGISwapchain
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    public const DXGI_FORMAT DefaultFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

    public ComPtr<IDXGISwapChain3> Swapchain;
    public ComPtr<ID3D12Fence> Fence;

    public HANDLE FenceEvent;
    public uint BackbufferFrameIndex;

    public ulong GPUFrame;
    //public ulong CPUFrame;

    public uint SyncInterval;
    public uint PresentFlags;


    private Inline3<D3D12DescriptorHandle> RenderTargetViews;
    private Inline3<ComPtr<ID3D12Resource>> Resources;

    public DXGI_FORMAT Format => DefaultFormat;
    public Handle<Texture> CurrentBackbuffer;


    [System(SystemStage.PreInit)]
    public static void PreInit(DXGISwapchain* swapchain, in D3D12ResourceManager resourceManager)
    {
        // add a slot for the backbuffer texture handle, this is used by other systems so we should set it up early.
        swapchain->CurrentBackbuffer = resourceManager.CreateTextureHandle();
    }

    [System(SystemStage.Init)]
    public static void Init(DXGISwapchain* swapchain, in Window window, in D3D12CommandQueue commandQueue, in D3D12Device device, in D3D12Allocator allocator, in D3D12ResourceManager resourceManager, IConfigurationManager configurationManager)
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

        var tearingSupport = config.AllowTearing && HasTearingSupport(factory);

        var width = (uint)window.Width;
        var height = (uint)window.Height;
        DXGI_SWAP_CHAIN_DESC1 desc = new()
        {
            BufferCount = BufferCount,
            AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_UNSPECIFIED,
            BufferUsage = DXGI_USAGE.DXGI_CPU_ACCESS_NONE,
            Flags = tearingSupport ? DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0,
            Format = DefaultFormat,
            Height = height,
            Width = width,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Scaling = DXGI_SCALING.DXGI_SCALING_NONE,
            Stereo = false,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD
        };
        hr = factory.Get()->CreateSwapChainForHwnd((IUnknown*)commandQueue.GetQueue(), window.Handle, &desc, null, null, (IDXGISwapChain1**)swapchain->Swapchain.GetAddressOf());
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

        if (!swapchain->InitBackbuffers(device, allocator, true))
        {
            Logger.Error<DXGISwapchain>("Failed to init backbuffers. FATAL");
        }

        swapchain->SyncInterval = config.VSync ? 1u : 0u;
        swapchain->PresentFlags = (uint)(tearingSupport && !config.VSync /*&& !Fullscreen*/ ? DXGI_PRESENT.DXGI_PRESENT_ALLOW_TEARING : 0);
    }

    private bool InitBackbuffers(in D3D12Device device, in D3D12Allocator allocator, bool createDescriptor)
    {
        for (var i = 0; i < BufferCount; ++i)
        {
            var hr = Swapchain.Get()->GetBuffer((uint)i, ID3D12Resource.Guid, (void**)Resources[i].GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<DXGISwapchain>($"Failed to get backbuffer at index {i}. HRESULT = {hr}");
                return false;
            }
            D3D12Helpers.SetName(Resources[i], $"Backbuffer_{i}");

            if (createDescriptor)
            {
                var rtvDescriptor = RenderTargetViews[i] = allocator.Allocate(DescriptorHeapType.RenderTargetView);
                device.CreateRenderTargetView(Resources[i], null, rtvDescriptor.CPU);
            }
        }
        BackbufferFrameIndex = Swapchain.Get()->GetCurrentBackBufferIndex();
        return true;
    }

    private bool ResizeBuffers(in Size size, in D3D12Device device, in D3D12CommandQueue commandQueue, in D3D12Allocator allocator)
    {
        // Start by flushing the GPU so no resources are in use.
        FlushGPU(commandQueue);

        // Release current buffers
        for (var i = 0; i < BufferCount; ++i)
        {
            Resources[i].Dispose();
            allocator.Free(RenderTargetViews[i]);
        }

        var hr = Swapchain.Get()->ResizeBuffers(BufferCount, (uint)size.Width, (uint)size.Height, DefaultFormat, 0);
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to resize the buffers. Width = {size.Width} Height = {size.Height} Format = {DefaultFormat} HRESULT = {hr}");
            return false;
        }

        if (!InitBackbuffers(device, allocator, true))
        {
            Logger.Error<DXGISwapchain>("Failed to init the buffers after resize.");
            return false;
        }

        return true;
    }

    private static bool HasTearingSupport(IDXGIFactory7* factory)
    {
        Debug.Assert(factory != null);
        uint tearing;
        var hr = factory->CheckFeatureSupport(DXGI_FEATURE.DXGI_FEATURE_PRESENT_ALLOW_TEARING, &tearing, sizeof(uint));
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Failed to check for {DXGI_FEATURE.DXGI_FEATURE_PRESENT_ALLOW_TEARING}. HRESULT = {hr}");
            return false;
        }

        return tearing != 0;
    }

    [System(SystemStage.First, SystemExecutionType.Inline)]
    public static void First(ref DXGISwapchain swapchain, in D3D12ResourceManager resourceManager, in Window window)
    {
        var texture = resourceManager.Access(swapchain.CurrentBackbuffer);
        texture->Format = DefaultFormat;
        texture->RTV = swapchain.RenderTargetViews[swapchain.BackbufferFrameIndex];
        texture->Resource = swapchain.Resources[swapchain.BackbufferFrameIndex];
        texture->Height = (uint)window.Height;
        texture->Width = (uint)window.Width;
    }

    [System(SystemStage.Last)]
    public static void Update(DXGISwapchain* swapchain, in D3D12CommandQueue queue, in D3D12Allocator allocator, in D3D12Device device, EventReader<WindowResizeEvent> resizeEvent)
    {
        swapchain->Present(queue);
        if (!EngineState.Active)
        {
            swapchain->FlushGPU(queue);
            return;
        }

        // we check for resize events after we've completed the draw. If a resize event happened we recreate the swapchain.
        if (resizeEvent.HasEvents)
        {
            foreach (ref readonly var @event in resizeEvent)
            {
                swapchain->ResizeBuffers(@event.Size, device, queue, allocator);
                break;
            }
        }
    }

    private void Present(in D3D12CommandQueue queue)
    {
        //var flags = TearingSupport && !Vsync && !Fullscreen ? DXGI_PRESENT.DXGI_PRESENT_ALLOW_TEARING : 0;
        var hr = Swapchain.Get()->Present(SyncInterval, PresentFlags);


        var frameCount = EngineState.FrameCount;
        var fence = Fence.Get();
        queue.Signal(fence, frameCount);
        var diff = frameCount - GPUFrame;
        if (diff >= BufferCount)
        {
            var waitFrame = GPUFrame + 1;
            if (fence->GetCompletedValue() < waitFrame)
            {
                fence->SetEventOnCompletion(waitFrame, FenceEvent);
                Kernel32.WaitForSingleObject(FenceEvent, INFINITE);
            }
            GPUFrame = fence->GetCompletedValue();
        }
#if DEBUG
        if (FAILED(hr))
        {
            Logger.Error<DXGISwapchain>($"Swapchain FAiled. HRESULT = {hr}");
            Debugger.Launch();
        }
#endif
        //Debug.Assert(SUCCEEDED(hr));
        BackbufferFrameIndex = (BackbufferFrameIndex + 1) % BufferCount;
    }

    private void FlushGPU(in D3D12CommandQueue queue)
    {
        //NOTE(Jens): this method can be used for resizing the buffers as well. 

        var frameCount = EngineState.FrameCount;
        for (var i = 0u; i < BufferCount; ++i)
        {
            //CPUFrame++;
            queue.Signal(Fence, frameCount);
            if (Fence.Get()->GetCompletedValue() < frameCount)
            {
                Fence.Get()->SetEventOnCompletion(frameCount, FenceEvent);
                Kernel32.WaitForSingleObject(FenceEvent, INFINITE);
            }
        }
        BackbufferFrameIndex = Swapchain.Get()->GetCurrentBackBufferIndex();
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(DXGISwapchain* swapchain, in D3D12CommandQueue commandQueue, in D3D12ResourceManager resourceManager, in D3D12Allocator allocator)
    {
        swapchain->FlushGPU(commandQueue);

        Kernel32.CloseHandle(swapchain->FenceEvent);
        for (var i = 0; i < BufferCount; ++i)
        {
            swapchain->Resources[i].Dispose();
            allocator.Free(swapchain->RenderTargetViews[i]);
        }

        //NOTE(Jens): This explodes.
        //*resourceManager.Access(swapchain->CurrentBackbuffer) = default;
        //resourceManager.DestroyTexture(swapchain->CurrentBackbuffer);

        swapchain->Fence.Dispose();
        swapchain->Swapchain.Dispose();
        *swapchain = default;
    }
}
