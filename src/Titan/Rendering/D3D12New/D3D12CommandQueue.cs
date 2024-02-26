using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.D3D12New;

[UnmanagedResource]
internal unsafe partial struct D3D12CommandQueue
{
    public const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
    public const uint CommandListCount = GlobalConfiguration.CommandBufferCount;

    public const uint TotalCommandListCount = BufferCount * CommandListCount;

    public ComPtr<ID3D12CommandQueue> Queue;

    public Inline3<Inline16<ComPtr<ID3D12CommandAllocator>>> Allocators;
    public Inline3<Inline16<ComPtr<ID3D12GraphicsCommandList4>>> CommandLists;

    [System(SystemStage.Init)]
    public static void Init(in D3D12Device device, D3D12CommandQueue* commandQueue)
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
    }

    [System(SystemStage.Shutdown)]
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
}
