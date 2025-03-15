using System.Runtime.CompilerServices;
using Titan.Application;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Titan.Graphics.D3D12;

[UnmanagedResource]
internal unsafe partial struct D3D12CommandQueue
{
    private struct QueuedCommandList
    {
        public Inline8<Ptr<ID3D12CommandList>> CommandLists;
        public uint Count;
    }

    private const uint BufferCount = GlobalConfiguration.MaxRenderFrames;

    // Direct Command Lists
    private const uint DirectCommandListCount = GlobalConfiguration.CommandBufferCount;
    private const uint TotalDirectCommandListCount = BufferCount * DirectCommandListCount;

    private ComPtr<ID3D12CommandQueue> Queue;
    private Inline3<Inline16<ComPtr<ID3D12CommandAllocator>>> Allocators;
    private Inline3<Inline16<ComPtr<ID3D12GraphicsCommandList4>>> CommandLists;
    private Inline16<QueuedCommandList> QueuedCommandLists;
    private int QueuedCommandListCount;
    private uint Next;

    // Compute Command Lists
    private const uint ComputeCommandListCount = GlobalConfiguration.ComputeBufferCount;
    private const uint TotalComputeCommandListCount = BufferCount * ComputeCommandListCount;
    private ComPtr<ID3D12CommandQueue> ComputeQueue;
    private Inline3<Inline4<ComPtr<ID3D12CommandAllocator>>> ComputeAllocators;
    private Inline3<Inline4<ComPtr<ID3D12GraphicsCommandList4>>> ComputeCommandLists;
    private Inline16<QueuedCommandList> QueuedComputeCommandLists;
    private int QueuedComputeCommandListCount;
    private uint ComputeNext;

    public readonly ID3D12CommandQueue* GetQueue() => Queue.Get();
    private D3D12ResourceManager* ResourceManager;

    [System(SystemStage.PreInit)]
    public static void Init(in D3D12Device device, D3D12CommandQueue* commandQueue, UnmanagedResourceRegistry registry)
    {
        //TODO(Jens): We create a lot of allocators/lists here. Maybe we should consider lazy creation.

        using var _ = new MeasureTime<D3D12CommandQueue>("Init completed in {0} ms");
        commandQueue->Queue = device.CreateCommandQueue(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT, "DirectCommandQueue");
        if (!commandQueue->Queue.IsValid)
        {
            Logger.Error<D3D12CommandQueue>("Failed to create the direct command queue.");
            return;
        }

        commandQueue->ComputeQueue = device.CreateCommandQueue(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COMPUTE, "ComputeCommandQueue");
        if (!commandQueue->ComputeQueue.IsValid)
        {
            Logger.Error<D3D12CommandQueue>("Failed to create the compute command queue.");
            return;
        }

        CreateCommandListsAndAllocators(
            device,
            (ComPtr<ID3D12CommandAllocator>*)commandQueue->Allocators.AsPointer(),
            (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->CommandLists.AsPointer(),
            TotalDirectCommandListCount,
            D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT
        );

        CreateCommandListsAndAllocators(
            device,
            (ComPtr<ID3D12CommandAllocator>*)commandQueue->ComputeAllocators.AsPointer(),
            (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->ComputeCommandLists.AsPointer(),
            TotalComputeCommandListCount,
            D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COMPUTE
        );

        commandQueue->ResourceManager = registry.GetResourcePointer<D3D12ResourceManager>();

        static void CreateCommandListsAndAllocators(in D3D12Device device, ComPtr<ID3D12CommandAllocator>* allocators, ComPtr<ID3D12GraphicsCommandList4>* commandLists, uint count, D3D12_COMMAND_LIST_TYPE type)
        {
            for (var i = 0; i < count; ++i)
            {
                var commandList = device.CreateCommandList(type, $"{type}[{i}]");
                if (commandList == null)
                {
                    Logger.Error<D3D12CommandQueue>($"Failed to create the {type} Command List at index {i}");
                }

                var allocator = device.CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT, $"{type}[{i}]");
                if (allocator == null)
                {
                    Logger.Error<D3D12CommandQueue>($"Failed to create the {type} Command Allocator at index {i}");
                }

                allocators[i] = allocator;
                commandLists[i] = commandList;
            }
        }
    }

    [System(SystemStage.Last)]
    public static void ExecuteDirectCommandLists(ref D3D12CommandQueue commandQueue)
    {
        //TODO(Jens): Would be nice to separate these, but the DXGISwapchain must be run after, so we need to change how the scheduler works to support it. The workaround now is to make this have a ref dependency to the Command Queue
        var queue = commandQueue.Queue.Get();
        for (var i = 0; i < commandQueue.QueuedCommandListCount; ++i)
        {
            var queuedItem = commandQueue.QueuedCommandLists.GetPointer(i);
            queue->ExecuteCommandLists(queuedItem->Count, (ID3D12CommandList**)queuedItem->CommandLists.AsPointer());
        }

        var computeQueue = commandQueue.ComputeQueue.Get();
        for (var i = 0; i < commandQueue.QueuedComputeCommandListCount; ++i)
        {
            var queuedItem = commandQueue.QueuedComputeCommandLists.GetPointer(i);
            computeQueue->ExecuteCommandLists(queuedItem->Count, (ID3D12CommandList**)queuedItem->CommandLists.AsPointer());
        }

        commandQueue.Next = 0;
        commandQueue.ComputeNext = 0;
        commandQueue.QueuedCommandListCount = 0;
        commandQueue.QueuedComputeCommandListCount = 0;
    }

    [System(SystemStage.EndOfLife)]
    public static void Shutdown(D3D12CommandQueue* commandQueue)
    {
        var allocators = (ComPtr<ID3D12CommandAllocator>*)commandQueue->Allocators.AsPointer();
        var commandLists = (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->CommandLists.AsPointer();

        for (var i = 0; i < TotalDirectCommandListCount; ++i)
        {
            allocators[i].Dispose();
            commandLists[i].Dispose();
        }

        var computeAllocators = (ComPtr<ID3D12CommandAllocator>*)commandQueue->ComputeAllocators.AsPointer();
        var computeCommandLists = (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->ComputeCommandLists.AsPointer();
        for (var i = 0; i < TotalComputeCommandListCount; ++i)
        {
            computeAllocators[i].Dispose();
            computeCommandLists[i].Dispose();
        }

        commandQueue->Queue.Dispose();
        commandQueue->ComputeQueue.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly HRESULT Signal(ID3D12Fence* fence, ulong value) => Queue.Get()->Signal(fence, value);


    public CommandList GetCommandList(Handle<PipelineState> handle)
    {
        ID3D12PipelineState* pipelineState = handle.IsValid
            ? ResourceManager->Access(handle)->Resource
            : null;

        return GetCommandList(pipelineState);

    }
    public CommandList GetCommandList(ID3D12PipelineState* pipelineState = null)
    {
        var index = Interlocked.Increment(ref Next) - 1;
        var commandList = CommandLists[EngineState.FrameIndex][index].Get();
        var allocator = Allocators[EngineState.FrameIndex][index].Get();
        allocator->Reset();
        commandList->Reset(allocator, pipelineState);
        return new(commandList);
    }

    public CommandList GetComputeCommandList(Handle<PipelineState> handle)
    {
        ID3D12PipelineState* pipelineState = handle.IsValid
            ? ResourceManager->Access(handle)->Resource
            : null;

        return GetComputeCommandList(pipelineState);

    }

    public CommandList GetComputeCommandList(ID3D12PipelineState* pipelineState = null)
    {
        var index = Interlocked.Increment(ref ComputeNext) - 1;
        var commandList = ComputeCommandLists[EngineState.FrameIndex][index].Get();
        var allocator = ComputeAllocators[EngineState.FrameIndex][index].Get();
        allocator->Reset();
        commandList->Reset(allocator, pipelineState);
        return new(commandList);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ExecuteCommandLists(ReadOnlySpan<CommandList> commandLists)
    {
        var index = Interlocked.Increment(ref Unsafe.AsRef(in QueuedCommandListCount)) - 1;

        var queuedCommandList = QueuedCommandLists.GetPointer(index);
        MemoryUtils.Copy(queuedCommandList->CommandLists.AsPointer(), commandLists);
        queuedCommandList->Count = (uint)commandLists.Length;

        ////TODO(Jens): Add some state tracking and validation that we've supplied correct command lists for the frame.
        //fixed (CommandList* ptr = commandLists)
        //{
        //    Queue.Get()->ExecuteCommandLists((uint)commandLists.Length, (ID3D12CommandList**)ptr);
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ExecuteCommandList(CommandList commandList)
    {
        Queue.Get()->ExecuteCommandLists(1, (ID3D12CommandList**)&commandList);
    }
}

