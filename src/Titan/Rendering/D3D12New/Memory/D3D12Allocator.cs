using System.Diagnostics;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.D3D12;
using Titan.Rendering.D3D12.Memory;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.D3D12New.Memory;

[UnmanagedResource]
internal unsafe partial struct D3D12Allocator
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;

    private D3D12DescriptorHeaps* _heaps;
    private int _frameIndex;

    [System(SystemStage.Init)]
    public static void Init(D3D12Allocator* allocator, D3D12DescriptorHeaps* heaps, ref readonly D3D12Device device, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var memoryConfig = config.MemoryConfig;

        if (!memoryManager.TryAllocArray(out heaps->SharedFreeList, memoryConfig.TotalCount))
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

            ref var heap = ref heaps->Heaps[(int)type];

            heap.NumberOfDescriptors = memoryConfig.GetDescriptorCount(type);
            heap.FreeList = heaps->SharedFreeList.Slice(offset, heap.NumberOfDescriptors);
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

        allocator->_heaps = heaps;
    }


    public readonly DescriptorHandle Allocate(DescriptorHeapType type)
    {
        ref var heap = ref _heaps->Heaps[(int)type];
        var index = heap.Count++;
        Debug.Assert(index >= 0);
        var offset = (uint)(heap.FreeList[index] * heap.IncrementSize);

        var cpuStart = heap.CPUStart.ptr + offset;
        var gpuStart = heap.ShaderVisible ? heap.GPUStart.ptr + offset : 0ul;

        return new(type, cpuStart, gpuStart, index);
    }


    public readonly void Free(in DescriptorHandle handle)
    {
        ref var heap = ref _heaps->Heaps[(int)handle.Type];
        var index = --heap.Count;

        heap.FreeList[index] = handle.Index;
        //TODO(Jens): Add debug check for returning the same handle multiple times.
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(D3D12Allocator* allocator, D3D12DescriptorHeaps* heaps, IMemoryManager memoryManager)
    {
        for (var type = 0; type < (int)DescriptorHeapType.Count; ++type)
        {
            heaps->Heaps[type].Heap.Dispose();
        }

        memoryManager.FreeArray(ref heaps->SharedFreeList);
        *heaps = default;
        *allocator = default;
    }

    [System(SystemStage.Update, SystemExecutionType.Inline)]
    public static void TestUpdate(in D3D12Allocator allocator)
    {
        var a = allocator.Allocate(DescriptorHeapType.DepthStencilView);
        var b = allocator.Allocate(DescriptorHeapType.RenderTargetView);
        var c = allocator.Allocate(DescriptorHeapType.ShaderResourceView);
        var d = allocator.Allocate(DescriptorHeapType.UnorderedAccessView);

        allocator.Free(a);
        allocator.Free(b);
        allocator.Free(c);
        allocator.Free(d);
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
