using System.Runtime.CompilerServices;
using Titan.Application.Services;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using static Titan.Rendering.D3D12.Utils.D3D12Helpers;

namespace Titan.Rendering.D3D12;

using CommandListType = ID3D12GraphicsCommandList4;
internal sealed unsafe class D3D12CommandQueue : IService
{
    private const uint BufferCount = GlobalConfiguration.MaxRenderFrames;

    //NOTE(Jens): Add this to the configuration, no need to create these up front if they'll never be used.
    private const uint CommandListCount = 16;
    private const uint MaxCommandLists = BufferCount * CommandListCount;

    private ComPtr<ID3D12CommandQueue> _commandQueue;

    private Inline3<Inline16<ComPtr<ID3D12CommandAllocator>>> _allocators;
    private Inline3<Inline16<ComPtr<CommandListType>>> _commandLists;

    private uint _next;
    private uint _bufferIndex;

    public ID3D12CommandQueue* CommandQueue => _commandQueue;
    public bool Init(D3D12Device device)
    {
        var directQueue = device.CreateCommandQueue(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
        if (directQueue == null)
        {
            Logger.Error<D3D12CommandQueue>($"Failed to create the {nameof(ID3D12CommandQueue)}.");
            return false;
        }

        SetName(directQueue, $"{nameof(D3D12CommandQueue)}.{nameof(ID3D12CommandQueue)}");

        var allocators = (ComPtr<ID3D12CommandAllocator>*)_allocators.AsPointer();
        var commandLists = (ComPtr<CommandListType>*)_commandLists.AsPointer();
        for (var i = 0; i < MaxCommandLists; ++i)
        {
            var commandList = device.CreateCommandList(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
            if (commandList == null)
            {
                Logger.Error<D3D12CommandQueue>($"Failed to create a {nameof(CommandListType)}. Index = {i}");
                return false;
            }

            var allocator = device.CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT);
            if (allocator == null)
            {
                Logger.Error<D3D12CommandQueue>($"Failed to create a {nameof(ID3D12CommandAllocator)}. Index = {i}");
                return false;
            }

            SetName(commandList, $"{nameof(D3D12CommandQueue)}.{nameof(CommandListType)}[{i}]");
            SetName(allocator, $"{nameof(D3D12CommandQueue)}.{nameof(ID3D12CommandAllocator)}[{i}]");
            *commandLists = commandList;
            *allocators = allocator;

            commandLists++;
            allocators++;
        }

        _commandQueue = directQueue;
        return true;
    }


    //public D3D12CommandList GetCommandList()
    //{
    //    Debug.Assert(_resourceManager != null);
    //    var index = Interlocked.Increment(ref _next) - 1;
    //    Debug.Assert(index < CommandListCount, "Max command list count exceeded.");

    //    var allocator = _allocators[_bufferIndex][index];
    //    var commandList = _commandLists[_bufferIndex][index];

    //    Debug.Assert(allocator.IsValid && commandList.IsValid);
    //    return new D3D12CommandList(commandList, allocator, _resourceManager);
    //}

    public void ExecuteAndReset()
    {
        var queue = _commandQueue.Get();

        //NOTE(Jens): This will just execute all command lists, no dependencies. Change this when needed for postprocessing for example.
        //queue->ExecuteCommandLists(_next, (ID3D12CommandList**)_commandLists[_bufferIndex].AsPointer());

        _bufferIndex = (_bufferIndex + 1) % BufferCount;
        _next = 0;

    }

    public void Shutdown()
    {
        _commandQueue.Dispose();
        _commandQueue = default;

        var allocators = EnumerateAllocators();
        var commandLists = EnumerateCommandLists();
        for (var i = 0; i < MaxCommandLists; ++i)
        {
            allocators[i].Dispose();
            commandLists[i].Dispose();
        }

        _allocators = default;
        _commandLists = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT Signal(ID3D12Fence* fence, ulong value)
        => _commandQueue.Get()->Signal(fence, value);

    private Span<ComPtr<ID3D12CommandAllocator>> EnumerateAllocators()
        => new(_allocators.AsPointer(), (int)(MaxCommandLists * sizeof(ComPtr<ID3D12CommandAllocator>)));

    private Span<ComPtr<ID3D12GraphicsCommandList>> EnumerateCommandLists()
        => new(_commandLists.AsPointer(), (int)(MaxCommandLists * sizeof(ComPtr<ID3D12GraphicsCommandList>)));

}
