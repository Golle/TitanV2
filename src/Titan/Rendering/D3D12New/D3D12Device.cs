using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.D3D12;
using Titan.Rendering.D3D12.Memory;
using Titan.Rendering.D3D12New.Adapters;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12New;

[UnmanagedResource]
internal unsafe partial struct D3D12Device
{
    public ComPtr<ID3D12Device4> Device;

    public static implicit operator ID3D12Device4*(in D3D12Device device) => device.Device.Get();

    [System(SystemStage.Init)]
    public static void Init(D3D12Device* device, in D3D12Adapter d3d12Adapter, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        ref readonly var adapter = ref d3d12Adapter.PrimaryAdapter;

        Logger.Trace<D3D12Device>($"Creating a {nameof(ID3D12Device4)} with FeatureLevel {config.FeatureLevel}.");

        using var _ = new MeasureTime<D3D12Device>("Created device in {0} ms.");
        var hr = D3D12Common.D3D12CreateDevice((IUnknown*)adapter.Adapter.Get(), config.FeatureLevel, ID3D12Device4.Guid, (void**)device->Device.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12Device4)} with feature level {config.FeatureLevel}. HRESULT = {hr}");
        }
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(D3D12Device* device)
    {
        Logger.Trace<D3D12Device>($"Destroying the {nameof(ID3D12Device4)}.");
        device->Device.Dispose();
    }




    public readonly ID3D12CommandQueue* CreateCommandQueue(D3D12_COMMAND_LIST_TYPE type)
    {
        var device = Device.Get();
        D3D12_COMMAND_QUEUE_DESC desc = new()
        {
            Flags = D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE,
            NodeMask = 0,
            Priority = 0,
            Type = type
        };
        ID3D12CommandQueue* queue;
        var hr = device->CreateCommandQueue(&desc, ID3D12CommandQueue.Guid, (void**)&queue);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12CommandQueue)}. HRESULT = {hr}");
            return null;
        }

        return queue;
    }

    public readonly ID3D12GraphicsCommandList4* CreateGraphicsCommandList(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12GraphicsCommandList4* commandList;
        var hr = Device.Get()->CreateCommandList1(0, type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, ID3D12GraphicsCommandList4.Guid, (void**)&commandList);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12GraphicsCommandList4)}. HRESULT = {hr}");
            return null;
        }

        return commandList;
    }

    public readonly ID3D12CommandAllocator* CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12CommandAllocator* allocator;
        var hr = Device.Get()->CreateCommandAllocator(type, ID3D12CommandAllocator.Guid, (void**)&allocator);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12CommandAllocator)}. HRESULT = {hr}");
            return null;
        }

        return allocator;
    }

    public readonly ID3D12Fence* CreateFence()
    {
        ID3D12Fence* fence;
        var hr = Device.Get()->CreateFence(0, D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE, ID3D12Fence.Guid, (void**)&fence);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Fence)}. HRESULT = {hr}");
            return null;
        }
        return fence;
    }

    public readonly ID3D12DescriptorHeap* CreateDescriptorHeap(D3D12_DESCRIPTOR_HEAP_TYPE type, uint count, bool shaderVisible)
    {
        ID3D12DescriptorHeap* heap;
        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Flags = shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : 0,
            NodeMask = 0,
            NumDescriptors = count,
            Type = type
        };
        var hr = Device.Get()->CreateDescriptorHeap(&desc, ID3D12DescriptorHeap.Guid, (void**)&heap);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12DescriptorHeap)}. HRESULT = {hr}");
            return null;
        }

        return heap;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetDescriptorHandleIncrementSize(DescriptorHeapType type)
    {
        var d3d12Type = type switch
        {
            DescriptorHeapType.ShaderResourceView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
            DescriptorHeapType.RenderTargetView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV,
            DescriptorHeapType.DepthStencilView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV,
            DescriptorHeapType.UnorderedAccessView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
            DescriptorHeapType.Count => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return Device.Get()->GetDescriptorHandleIncrementSize(d3d12Type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE type)
        => Device.Get()->GetDescriptorHandleIncrementSize(type);
}
