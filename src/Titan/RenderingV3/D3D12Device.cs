using System;
using System.Diagnostics;
using System.Xml.Linq;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.RenderingV3;

internal static unsafe class D3D12Device
{
    public static ID3D12CommandQueue* CreateCommandQueue(ID3D12Device4* device, D3D12_COMMAND_LIST_TYPE type, string? name = null)
    {
        var desc = new D3D12_COMMAND_QUEUE_DESC
        {
            Type = type,
            Flags =
                D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE,
            NodeMask = 0,
            Priority = 0
        };
        ID3D12CommandQueue* commandQueue;
        var hr = device->CreateCommandQueue(&desc, ID3D12CommandQueue.Guid, (void**)&commandQueue);

        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(commandQueue, name);
            return commandQueue;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12CommandQueue)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }

    public static ID3D12DescriptorHeap* CreateDescriptorHeap(ID3D12Device4* device, D3D12_DESCRIPTOR_HEAP_TYPE type, uint numberOfDescriptors, D3D12_DESCRIPTOR_HEAP_FLAGS flags, string? name = null)
    {
        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Type = type,
            NumDescriptors = numberOfDescriptors,
            Flags = flags,
            NodeMask = 0
        };

        ID3D12DescriptorHeap* heap;
        var hr = device->CreateDescriptorHeap(&desc, ID3D12DescriptorHeap.Guid, (void**)&heap);
        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(heap, name);
            return heap;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12DescriptorHeap)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }


    public static uint GetDescriptorHandleIncrementSize(ID3D12Device4* device, D3D12_DESCRIPTOR_HEAP_TYPE type)
        => device->GetDescriptorHandleIncrementSize(type);


    public static ID3D12Fence* CreateFence(ID3D12Device4* device, string? name = null)
    {
        ID3D12Fence* fence;
        var hr = device->CreateFence(0, D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE, ID3D12Fence.Guid, (void**)&fence);
        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(fence, name);
            return fence;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12DescriptorHeap)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }

    public static void CreateRenderTargetView(ID3D12Device4* device, ID3D12Resource* resource, D3D12_RENDER_TARGET_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        Debug.Assert(device != null);
        Debug.Assert(resource != null);
        device->CreateRenderTargetView(resource, &desc, cpuHandle);
    }

    public static void CreateShaderResourceView(ID3D12Device4* device, ID3D12Resource* resource, D3D12_SHADER_RESOURCE_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        Debug.Assert(device != null);
        Debug.Assert(resource != null);
        device->CreateShaderResourceView(resource, &desc, cpuHandle);
    }

    public static void CreateRenderTargetView(ID3D12Device4* device, ID3D12Resource* resource, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        Debug.Assert(device != null);
        Debug.Assert(resource != null);
        device->CreateRenderTargetView(resource, null, cpuHandle);
    }

    public static void CreateUnorderedAccessView(ID3D12Device4* device, ID3D12Resource* resource, D3D12_UNORDERED_ACCESS_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        Debug.Assert(device != null);
        Debug.Assert(resource != null);
        //TODO(Jens): Do we need the second parameter?
        device->CreateUnorderedAccessView(resource, null, &desc, cpuHandle);
    }

    public static ID3D12GraphicsCommandList4* CreateCommandList(ID3D12Device4* device, D3D12_COMMAND_LIST_TYPE type, string? name = null)
    {
        ID3D12GraphicsCommandList4* commandList;
        var hr = device->CreateCommandList1(0, type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, ID3D12GraphicsCommandList4.Guid, (void**)&commandList);
        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(commandList, name);
            return commandList;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12GraphicsCommandList4)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }

    public static ID3D12CommandAllocator* CreateCommandAllocator(ID3D12Device4* device, D3D12_COMMAND_LIST_TYPE type, string? name = null)
    {
        ID3D12CommandAllocator* allocator;
        var hr = device->CreateCommandAllocator(type, ID3D12CommandAllocator.Guid, (void**)&allocator);
        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(allocator, name);
            return allocator;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12CommandAllocator)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }

    public static ID3D12Resource* CreateResource(ID3D12Device4* device, D3D12_RESOURCE_DESC desc, D3D12_RESOURCE_STATES initialState, D3D12_HEAP_TYPE heapType, D3D12_HEAP_FLAGS heapFlags = D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE, D3D12_CLEAR_VALUE* clearValue = null, string? name = null)
    {
        var heap = D3D12Helpers.GetHeap(heapType);
        ID3D12Resource* resource;
        var hr = device->CreateCommittedResource1(heap, heapFlags, &desc, initialState, clearValue, null, ID3D12Resource.Guid, (void**)&resource);
        if (SUCCEEDED(hr))
        {
            D3D12Helpers.SetName(resource, name);
            return resource;
        }
        Logger.Error($"Failed to create the {nameof(ID3D12Resource)}. HRESULT = {hr}", typeof(D3D12Device));
        return null;
    }
}
