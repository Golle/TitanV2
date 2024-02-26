using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Rendering.D3D12.Memory;
internal unsafe struct DescriptorHeap
{
    private TitanArray<uint> _freeList;
    private D3D12_CPU_DESCRIPTOR_HANDLE _cpuStart;
    private D3D12_GPU_DESCRIPTOR_HANDLE _gpuStart;
    private uint _incrementSize;
    private uint _descriptorCount;
    // This is the number of descriptors in this heap (excluding the temporary ones)
    private uint _numberOfDescriptors;

    private uint _numberOfTempDescriptors;
    private volatile uint _tempOffset;
    private uint _frameIndex;

    private ComPtr<ID3D12DescriptorHeap> _heap;
    private DescriptorHeapType _type;
    private bool _shaderVisible;
    private SpinLock _lock;


    public bool Init(IMemoryManager memoryManager, D3D12Device device, DescriptorHeapType type, uint count, uint temporaryCount, bool shaderVisible)
    {
        if (!memoryManager.TryAllocArray(out _freeList, count))
        {
            Logger.Error<DescriptorHeap>($"Failed to allocate array. Count = {count} Size = {sizeof(uint)}");
            return false;
        }

        var d3d12type = type switch
        {
            DescriptorHeapType.DepthStencilView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV,
            DescriptorHeapType.RenderTargetView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV,
            DescriptorHeapType.ShaderResourceView or DescriptorHeapType.UnorderedAccessView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
            _ => throw new NotSupportedException($"The type {type} is not supported.")
        };


        var totalCount = GlobalConfiguration.MaxRenderFrames * temporaryCount + count;

        var heap = device.CreateDescriptorHeap(d3d12type, totalCount, shaderVisible);
        if (heap == null)
        {
            Logger.Error<DescriptorHeap>($"Failed to create the {nameof(ID3D12DescriptorHeap)}. Type = {d3d12type} Count = {totalCount} ShaderVisible = {shaderVisible}");
            return false;
        }

        //NOTE(Jens): This API is a bit weird, but this works :)
        D3D12_CPU_DESCRIPTOR_HANDLE cpuStart;
        _cpuStart = *heap->GetCPUDescriptorHandleForHeapStart(&cpuStart);
        if (shaderVisible)
        {
            D3D12_GPU_DESCRIPTOR_HANDLE gpuStart;
            _gpuStart = *heap->GetGPUDescriptorHandleForHeapStart(&gpuStart);
        }

        _incrementSize = device.GetDescriptorHandleIncrementSize(d3d12type);
        _shaderVisible = shaderVisible;
        _heap = heap;
        _descriptorCount = 0;
        _type = type;
        _numberOfDescriptors = count;
        _numberOfTempDescriptors = temporaryCount;
        for (var i = 0u; i < count; ++i)
        {
            _freeList[i] = i;
        }

        return true;
    }

    public DescriptorHandle Allocate()
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        var index = _freeList[_descriptorCount++];
        _lock.Exit();

        var offset = index * _incrementSize;
        var cpuStart = _cpuStart.ptr + offset;
        var gpuStart = _shaderVisible ? _gpuStart.ptr + offset : 0ul;

        return new(_type, cpuStart, gpuStart, (int)index);
    }

    public void Free(in DescriptorHandle handle)
    {
        Debug.Assert(handle.Type == _type, $"Expected type = {_type}. Got {handle.Type}");

#if DEBUG
        CheckForDuplicateIndices(_freeList.AsReadOnlySpan()[(int)_descriptorCount..], (uint)handle.Index);
#endif

        var gotLock = false;
        _lock.Enter(ref gotLock);
        _freeList[--_descriptorCount] = (uint)handle.Index;
        _lock.Exit();
    }

    [Conditional("DEBUG")]
    private static void CheckForDuplicateIndices(ReadOnlySpan<uint> span, uint index)
    {
        foreach (var value in span)
        {
            if (value == index)
            {
                Debug.Fail($"The index {index} has been freed multiple times.");
            }
        }
    }

    public void Shutdown(IMemoryManager memoryManager)
    {
        _heap.Dispose();
        memoryManager.FreeArray(ref _freeList);
        this = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TempDescriptorHandle AllocateTemp()
    {
        //NOTE(Jens): Move this to the initialization later
        var frameOffset = _frameIndex * _numberOfTempDescriptors;
        var offset = Interlocked.Increment(ref _tempOffset) - 1;

        var index = frameOffset + offset + _numberOfDescriptors;
        var offsetInBytes = index * _incrementSize;

        return new TempDescriptorHandle(index, _cpuStart.ptr + offsetInBytes, _gpuStart.ptr + offsetInBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EndFrame()
    {
        _frameIndex = (_frameIndex + 1) % GlobalConfiguration.MaxRenderFrames;
        _tempOffset = 0;
    }
}
