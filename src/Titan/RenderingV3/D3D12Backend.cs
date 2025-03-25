using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.Systems;
using Titan.Windows;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.RenderingV3;

internal unsafe partial struct D3D12Backend
{
    public const uint MaxFramesInFlight = GlobalConfiguration.MaxRenderFrames;

    [System(SystemStage.Init)]
    public static void Init(ref D3D12Context context, in Window window, IConfigurationManager configurationManager)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created the D3D12 Rendering Context. Elapsed = {0} ms.");
        Debug.Assert(window.Handle != 0);

        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var renderingConfig = configurationManager.GetConfigOrDefault<RenderingConfig>();

        CreateDevice(ref context, config);
        CreateCommandQueues(ref context);
        CreateCommandListsAndAllocators(ref context);
        CreateDescriptorHeaps(ref context, config);
        CreateSwapchain(ref context, window, renderingConfig);
        InitBackbuffers(ref context, true);
    }

    private static void CreateCommandListsAndAllocators(ref D3D12Context context)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created command lists and allocators. Elapsed = {0} ms.");
        Debug.Assert(context.CommandLists.Size >= MaxFramesInFlight);

        // Initialize the Direct Command Lists
        for (var frameIndex = 0; frameIndex < MaxFramesInFlight; ++frameIndex)
        {
            ref var commandList = ref context.CommandLists[frameIndex];
            for (var i = 0; i < commandList.CommandLists.Size; ++i)
            {
                commandList.CommandLists[i] = D3D12Device.CreateCommandList(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT, $"CommandList[{frameIndex}][{i}]");
                commandList.Allocators[i] = D3D12Device.CreateCommandAllocator(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT, $"CommandAllocator[{frameIndex}][{i}]");
            }
        }

        // Initialize the Copy Command Lists
        ref var copyCommandLists = ref context.CopyCommandLists;
        for (var i = 0; i < copyCommandLists.Allocator.Size; ++i)
        {
            copyCommandLists.CommandList[i] = D3D12Device.CreateCommandList(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY, $"CopyCommandList[{i}]");
            copyCommandLists.Allocator[i] = D3D12Device.CreateCommandAllocator(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY, $"CopyCommandAllocator[{i}]");
        }

        copyCommandLists.EventHandle = Kernel32.CreateEventW(null, 0, 0, "CopyCommandQueueFenceEvent");
        copyCommandLists.Fence = D3D12Device.CreateFence(context.Device, "CopyCommandQueueFence");
    }

    private static void InitBackbuffers(ref D3D12Context context, bool createDescriptors)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created backbuffers. Elapsed = {0} ms.");

        for (var i = 0u; i < D3D12Swapchain.BufferCount; ++i)
        {
            var hr = context.Swapchain.Swapchain.Get()->GetBuffer(i, ID3D12Resource.Guid, (void**)context.Swapchain.Backbuffers[i].GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<DXGISwapchain>($"Failed to get backbuffer at index {i}. HRESULT = {hr}");
            }
            D3D12Helpers.SetName(context.Swapchain.Backbuffers[i], $"Backbuffer[{i}]");

            if (createDescriptors)
            {
                var handle = context.AllocDescriptor(DescriptorHeapTypes.RenderTargetView);
                var cpuHandle = context.GetCpuDescriptorHandle(handle);
                D3D12Device.CreateRenderTargetView(context.Device, context.Swapchain.Backbuffers[i], cpuHandle);
            }
        }
    }

    private static void CreateSwapchain(ref D3D12Context context, in Window window, RenderingConfig config)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created Swapchain. Elapsed = {0} ms.");

        var flags = config.Debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;

        // create Factory
        using ComPtr<IDXGIFactory7> factory = default;
        var hresult = DXGICommon.CreateDXGIFactory2(flags, factory.UUID, (void**)factory.GetAddressOf());
        if (FAILED(hresult))
        {
            Logger.Error<D3D12Backend>($"Failed to create the {nameof(IDXGIFactory7)}. HRESULT = {hresult}");
            Environment.Exit(1);
        }

        // Check for Tearing support
        uint tearing;
        hresult = factory.Get()->CheckFeatureSupport(DXGI_FEATURE.DXGI_FEATURE_PRESENT_ALLOW_TEARING, &tearing, sizeof(uint));
        if (FAILED(hresult))
        {
            Logger.Error<DXGISwapchain>($"Failed to check for {DXGI_FEATURE.DXGI_FEATURE_PRESENT_ALLOW_TEARING}. HRESULT = {hresult}");
            Environment.Exit(1);
        }


        // Create the swapchain
        var tearingSupport = config.AllowTearing && tearing != 0;
        var width = (uint)window.Width;
        var height = (uint)window.Height;
        DXGI_SWAP_CHAIN_DESC1 desc = new()
        {
            BufferCount = D3D12Swapchain.BufferCount,
            AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_UNSPECIFIED,
            BufferUsage = DXGI_USAGE.DXGI_CPU_ACCESS_NONE,
            Flags = tearingSupport ? DXGI_SWAP_CHAIN_FLAG.DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0,
            Format = D3D12Swapchain.DefaultFormat,
            Height = height,
            Width = width,
            SampleDesc = { Count = 1, Quality = 0 },
            Scaling = DXGI_SCALING.DXGI_SCALING_NONE,
            Stereo = false,
            SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_DISCARD
        };

        hresult = factory.Get()->CreateSwapChainForHwnd((IUnknown*)context.GetCommandQueue(CommandQueueTypes.Direct), window.Handle, &desc, null, null, (IDXGISwapChain1**)context.Swapchain.Swapchain.GetAddressOf());
        if (FAILED(hresult))
        {
            Logger.Error<DXGISwapchain>($"Falied to create the {nameof(IDXGISwapChain3)}. HRESULT = {hresult}");
            Environment.Exit(1);
        }

        // Create fences
        context.Swapchain.Fence = D3D12Device.CreateFence(context.Device, "SwapchainFence");
        Debug.Assert(context.Swapchain.Fence.IsValid);

        fixed (char* pName = $"{nameof(DXGISwapchain)}_FenceEvent")
        {
            context.Swapchain.FenceEvent = Kernel32.CreateEventW(null, 0, 0, pName);
        }

        // Disable Alt-enter (will be handled by windows input)
        {
            hresult = factory.Get()->MakeWindowAssociation(window.Handle, DXGI_MAKE_WINDOW_ASSOCIATION_FLAGS.DXGI_MWA_NO_ALT_ENTER);
            if (FAILED(hresult))
            {
                Logger.Error<DXGISwapchain>($"Failed to disable Alt+Enter. HRESULT = {hresult}");
            }
        }

        context.Swapchain.SyncInterval = config.VSync ? 1u : 0u;
        context.Swapchain.PresentFlags = (uint)(tearingSupport && !config.VSync /*&& !Fullscreen*/ ? DXGI_PRESENT.DXGI_PRESENT_ALLOW_TEARING : 0);
    }

    private static void CreateDescriptorHeaps(ref D3D12Context context, D3D12Config config)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created descriptor heaps. Elapsed = {0} ms.");
        var memoryConfig = config.MemoryConfig2;
        CreateHeap(ref context.DescriptorHeaps[(int)DescriptorHeapTypes.DepthStencilView], context.Device, DescriptorHeapTypes.DepthStencilView, memoryConfig.DSVCount, false, "Depth Stencil View Descriptor Heap");
        CreateHeap(ref context.DescriptorHeaps[(int)DescriptorHeapTypes.ShaderResourceView], context.Device, DescriptorHeapTypes.ShaderResourceView, memoryConfig.SRVCount, true, "Shader Resource View Descriptor Heap");
        CreateHeap(ref context.DescriptorHeaps[(int)DescriptorHeapTypes.UnorderedAccessView], context.Device, DescriptorHeapTypes.UnorderedAccessView, memoryConfig.UAVCount, true, "Unordered Access View Descriptor Heap");
        CreateHeap(ref context.DescriptorHeaps[(int)DescriptorHeapTypes.RenderTargetView], context.Device, DescriptorHeapTypes.RenderTargetView, memoryConfig.RTVCount, false, "Render Target View Descriptor Heap");

        static void CreateHeap(ref D3D12DescriptorHeap heap, ID3D12Device4* device, DescriptorHeapTypes type, uint count, bool shaderVisible, string name)
        {
            Debug.Assert(count < ushort.MaxValue);
            Debug.Assert(count <= heap.FreeList.Size, $"The max size is currently set to {heap.FreeList.Size}. Descriptor count is {count}. Decrease count or increase the inline buffer.");
            var flags = shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : 0;
            var heapType = type switch
            {
                DescriptorHeapTypes.DepthStencilView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV,
                DescriptorHeapTypes.RenderTargetView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV,
                _ => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV
            };
            heap.Resource = D3D12Device.CreateDescriptorHeap(device, heapType, count, flags, name);
            heap.IncrementSize = D3D12Device.GetDescriptorHandleIncrementSize(device, heapType);
            Debug.Assert(heap.Resource.IsValid);
            Debug.Assert(heap.IncrementSize > 0);

            heap.CPUStart = *heap.Resource.Get()->GetCPUDescriptorHandleForHeapStart(MemoryUtils.AsPointer(heap.CPUStart));
            if (shaderVisible)
            {
                heap.GPUStart = *heap.Resource.Get()->GetGPUDescriptorHandleForHeapStart(MemoryUtils.AsPointer(heap.GPUStart));
            }
            heap.Type = type;
            heap.MaxCount = (ushort)count;
            heap.ShaderVisibile = shaderVisible;

            for (var i = 0; i < count; ++i)
            {
                heap.FreeList[i] = (ushort)i;
            }
        }
    }

    private static void CreateCommandQueues(ref D3D12Context context)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created command queues. Elapsed = {0} ms.");

        context.CommandQueues[(int)CommandQueueTypes.Direct] = D3D12Device.CreateCommandQueue(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
        context.CommandQueues[(int)CommandQueueTypes.Copy] = D3D12Device.CreateCommandQueue(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY);
        context.CommandQueues[(int)CommandQueueTypes.Compute] = D3D12Device.CreateCommandQueue(context.Device, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COMPUTE);

        Debug.Assert(context.CommandQueues[(int)CommandQueueTypes.Direct].IsValid);
        Debug.Assert(context.CommandQueues[(int)CommandQueueTypes.Copy].IsValid);
        Debug.Assert(context.CommandQueues[(int)CommandQueueTypes.Compute].IsValid);
    }

    private static void CreateDevice(ref D3D12Context context, D3D12Config config)
    {
        using var _ = new MeasureTime<D3D12Backend>("Created D3D12 Device. Elapsed = {0} ms.");

        var hresult = D3D12Common.D3D12CreateDevice(null, config.FeatureLevel, context.Device.UUID, (void**)context.Device.GetAddressOf());
        if (FAILED(hresult))
        {
            Logger.Error<D3D12Backend>($"Failed to create the {nameof(ID3D12Device4)}. HRESULT = {hresult}");
            Environment.Exit(1);
        }
    }
}
