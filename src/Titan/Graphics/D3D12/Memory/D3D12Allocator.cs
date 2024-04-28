using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.D3D12.Memory;

[UnmanagedResource]
internal unsafe partial struct D3D12Allocator
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;

    private TitanArray<int> _sharedFreeList;
    private Inline4<DescriptorHeap> _heaps;

    private int _frameIndex;

    [UnscopedRef]
    public ref readonly DescriptorHeap SRV => ref _heaps[(int)DescriptorHeapType.ShaderResourceView];
    [System(SystemStage.PreInit)]
    public static void Init(D3D12Allocator* allocator, in D3D12Device device, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var memoryConfig = config.MemoryConfig;

        if (!memoryManager.TryAllocArray(out allocator->_sharedFreeList, memoryConfig.TotalCount))
        {
            Logger.Error<D3D12Allocator>($"Failed to allocate memory for the shared free list. Count = {memoryConfig.TotalCount} Size = {memoryConfig.TotalCount * sizeof(uint)}");
            return;
        }

        var offset = 0u;
        for (DescriptorHeapType type = 0; type < DescriptorHeapType.Count; ++type)
        {
            var d3d12Type = type switch
            {
                DescriptorHeapType.ShaderResourceView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
                DescriptorHeapType.DepthStencilView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV,
                DescriptorHeapType.RenderTargetView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV,
                DescriptorHeapType.UnorderedAccessView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
                _ => throw new InvalidOperationException("Not supported")
            };

            ref var heap = ref allocator->_heaps[(int)type];

            heap.NumberOfDescriptors = memoryConfig.GetDescriptorCount(type);
            heap.FreeList = allocator->_sharedFreeList.Slice(offset, heap.NumberOfDescriptors);
            heap.IncrementSize = device.GetDescriptorHandleIncrementSize(d3d12Type);
            heap.ShaderVisible = type is DescriptorHeapType.ShaderResourceView;
            //NOTE(Jens): We only have temporary SRV descriptors.
            heap.NumberOfTempDescriptors = type is DescriptorHeapType.ShaderResourceView ? memoryConfig.TempShaderResourceViewCount : 0;


            var totalCount = BufferCount * heap.NumberOfTempDescriptors + heap.NumberOfDescriptors;
            ID3D12DescriptorHeap* descriptorHeap = heap.Heap = device.CreateDescriptorHeap(d3d12Type, totalCount, heap.ShaderVisible);
            if (descriptorHeap == null)
            {
                Logger.Error<D3D12Allocator>($"Failed to create the {nameof(ID3D12DescriptorHeap)}. Type = {type} D3D12Type = {d3d12Type} Count = {totalCount} ShaderVisible = {heap.ShaderVisible}");
                // fatal
            }

            heap.CPUStart = *descriptorHeap->GetCPUDescriptorHandleForHeapStart(MemoryUtils.AsPointer(heap.CPUStart));
            if (heap.ShaderVisible)
            {
                heap.GPUStart = *descriptorHeap->GetGPUDescriptorHandleForHeapStart(MemoryUtils.AsPointer(heap.GPUStart));
            }

            // Init the free list with indices (we could pre-calculate and store the offsets instead)
            for (var i = 0; i < heap.NumberOfDescriptors; ++i)
            {
                heap.FreeList[i] = i;
            }

            offset += heap.NumberOfDescriptors;
        }
    }


    public readonly DescriptorHandle Allocate(DescriptorHeapType type)
    {
        ref var heap = ref *(_heaps.AsPointer() + (int)type);
        var index = heap.Count++;
        Debug.Assert(index >= 0);
        var offset = (uint)(heap.FreeList[index] * heap.IncrementSize);

        var cpuStart = heap.CPUStart.ptr + offset;
        var gpuStart = heap.ShaderVisible ? heap.GPUStart.ptr + offset : 0ul;

        return new(type, cpuStart, gpuStart, index);
    }


    public readonly void Free(in DescriptorHandle handle)
    {
        ref var heap = ref *(_heaps.AsPointer() + (int)handle.Type);
        var index = --heap.Count;

        heap.FreeList[index] = handle.Index;
        //TODO(Jens): Add debug check for returning the same handle multiple times.
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(D3D12Allocator* allocator, IMemoryManager memoryManager)
    {
        for (var type = 0; type < (int)DescriptorHeapType.Count; ++type)
        {
            allocator->_heaps[type].Heap.Dispose();
        }

        memoryManager.FreeArray(ref allocator->_sharedFreeList);
        *allocator = default;
    }

    /// <summary>
    /// Reset the temporary descriptor heaps at the end of the frame. This is a trivial action so we inline it (runs on main thread by the scheduler)
    /// </summary>
    [System(SystemStage.Last, SystemExecutionType.Inline)]
    public static void Update(ref D3D12Allocator allocator)
    {
        allocator._frameIndex = (int)((allocator._frameIndex + 1) % BufferCount);

        //for (var i = 0; i < (int)DescriptorHeapType.Count; ++i)
        //{
        //    //TODO(Jens): Implement reset of the temporary buffers (only SRV)
        //    //allocator->DescriptorHeaps[i].EndFrame();
        //}
    }


}
