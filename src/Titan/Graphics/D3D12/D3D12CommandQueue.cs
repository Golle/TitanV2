using System.Runtime.CompilerServices;
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
        public Inline4<Ptr<ID3D12CommandList>> CommandLists;
        public uint Count;
    }

    private const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    private const uint CommandListCount = GlobalConfiguration.CommandBufferCount;

    private const uint TotalCommandListCount = BufferCount * CommandListCount;

    private uint BufferIndex;
    private uint Next;

    private ComPtr<ID3D12CommandQueue> Queue;

    private Inline3<Inline16<ComPtr<ID3D12CommandAllocator>>> Allocators;
    private Inline3<Inline16<ComPtr<ID3D12GraphicsCommandList4>>> CommandLists;


    private Inline16<QueuedCommandList> QueuedCommandLists;
    private int QueuedCommandListCount;

    public readonly ID3D12CommandQueue* GetQueue() => Queue.Get();
    private D3D12ResourceManager* ResourceManager;

    [System(SystemStage.PreInit)]
    public static void Init(in D3D12Device device, D3D12CommandQueue* commandQueue, UnmanagedResourceRegistry registry)
    {
        using var _ = new MeasureTime<D3D12CommandQueue>("Init completed in {0} ms");
        commandQueue->Queue = device.CreateCommandQueue(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
        if (!commandQueue->Queue.IsValid)
        {
            Logger.Error<D3D12CommandQueue>("Failed to create the command queue.");
            return;
        }

        var allocators = (ComPtr<ID3D12CommandAllocator>*)commandQueue->Allocators.AsPointer();
        var commandLists = (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->CommandLists.AsPointer();

        for (var i = 0; i < TotalCommandListCount; ++i)
        {
            var commandList = device.CreateGraphicsCommandList(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
            if (commandList == null)
            {
                Logger.Error<D3D12CommandQueue>($"Failed to create the command list at index {i}");
            }

            var allocator = device.CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
            if (allocator == null)
            {
                Logger.Error<D3D12CommandQueue>($"Failed to create the command allocator at index {i}");
            }

            allocators[i] = allocator;
            commandLists[i] = commandList;
        }

        commandQueue->ResourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
    }

    [System(SystemStage.Last, SystemExecutionType.Inline)]
    public static void ExecuteAndReset(D3D12CommandQueue* commandQueue)
    {
        var queue = commandQueue->Queue.Get();
        for (var i = 0; i < commandQueue->QueuedCommandListCount; ++i)
        {
            var queuedItem = commandQueue->QueuedCommandLists.GetPointer(i);
            queue->ExecuteCommandLists(queuedItem->Count, (ID3D12CommandList**)queuedItem->CommandLists.AsPointer());
        }

        commandQueue->BufferIndex = (commandQueue->BufferIndex + 1) % BufferCount;
        commandQueue->Next = 0;
        commandQueue->QueuedCommandListCount = 0;
    }


    [System(SystemStage.EndOfLife)]
    public static void Shutdown(D3D12CommandQueue* commandQueue)
    {
        var allocators = (ComPtr<ID3D12CommandAllocator>*)commandQueue->Allocators.AsPointer();
        var commandLists = (ComPtr<ID3D12GraphicsCommandList4>*)commandQueue->CommandLists.AsPointer();

        for (var i = 0; i < TotalCommandListCount; ++i)
        {
            allocators[i].Dispose();
            commandLists[i].Dispose();
        }

        commandQueue->Queue.Dispose();
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
        var commandList = CommandLists[BufferIndex][index].Get();
        var allocator = Allocators[BufferIndex][index].Get();

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
