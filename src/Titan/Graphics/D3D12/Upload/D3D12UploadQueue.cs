using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.D3D12.Upload;

[UnmanagedResource]
internal unsafe partial struct D3D12UploadQueue
{
    private Inline8<UploadFrame> UploadFrames;
    private ComPtr<ID3D12CommandQueue> CommandQueue;
    private ComPtr<ID3D12Fence> Fence;
    private SpinLock FrameLock;
    private SpinLock QueueLock;

    private ulong FenceValue;
    private HANDLE FenceEvent;
    
    private D3D12Device* Device;

    [System(SystemStage.PreInit)]
    public static void Init(D3D12UploadQueue* queue, in D3D12Device device, UnmanagedResourceRegistry registry)
    {
        queue->CommandQueue = device.CreateCommandQueue(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY);
        queue->Fence = device.CreateFence();
        queue->FenceEvent = Kernel32.CreateEventW(null, 0, 0, $"{nameof(D3D12UploadQueue)}.{nameof(FenceEvent)}");
        queue->FenceValue = 0;
        queue->Device = registry.GetResourcePointer<D3D12Device>();

        for (var i = 0; i < queue->UploadFrames.Size; ++i)
        {
            ref var frame = ref queue->UploadFrames[i];
            frame.Allocator = device.CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY, $"{nameof(D3D12UploadQueue)}.{nameof(UploadFrames)}[{i}]");
            frame.CommandList = device.CreateGraphicsCommandList(D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY, $"{nameof(D3D12UploadQueue)}.{nameof(UploadFrames)}[{i}]");

            if (!frame.Allocator.IsValid)
            {
                Logger.Error<D3D12UploadQueue>($"Failed to create the {nameof(ID3D12CommandAllocator)} at index {i}");
            }

            if (!frame.CommandList.IsValid)
            {
                Logger.Error<D3D12UploadQueue>($"Failed to create the {nameof(ID3D12GraphicsCommandList4)} at index {i}");
            }
        }
    }

    public bool Upload(ID3D12Resource* destination, in TitanBuffer buffer)
    {
        Debug.Assert(destination != null && buffer.Size > 0);

        D3D12_HEAP_PROPERTIES heapProperties;
        destination->GetHeapProperties(&heapProperties, null);

        D3D12_RESOURCE_DESC resourceDesc;
        destination->GetDesc(&resourceDesc);

        if (heapProperties.Type is D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK or D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD)
        {
            // CPU visible resource, we can just map and copy.

            void* data;
            destination->Map(0, null, &data);
            MemoryUtils.Copy(data, buffer.AsReadOnlySpan());

            return true;
        }

        // Non CPU visible, we need to use a temporary buffer
        using ComPtr<ID3D12Resource> tempBuffer = Device->CreateBuffer(buffer.Size, true);

        if (!tempBuffer.IsValid)
        {
            Logger.Error<D3D12UploadQueue>($"Failed to create a temp buffer on the GPU. Size = {buffer.Size} bytes.");
            return false;
        }

        // Copy the buffer into the temp buffer.
        {
            void* data;
            var hr = tempBuffer.Get()->Map(0, null, &data);
            if (Win32Common.FAILED(hr) || data == null)
            {
                Logger.Error<D3D12UploadQueue>("Failed to map the temp buffer.");
                return false;
            }

            MemoryUtils.Copy(data, buffer.AsReadOnlySpan());
            tempBuffer.Get()->Unmap(0, null); // This might not be needed since we dispose the buffer at the end. 
        }

        var frame = GetAvailableFrame();
        frame->Allocator.Get()->Reset();

        var commandList = frame->CommandList.Get();
        commandList->Reset(frame->Allocator, null);

        if (resourceDesc.Dimension == D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER)
        {
            commandList->CopyBufferRegion(destination, 0, tempBuffer, 0, buffer.Size);
        }
        else
        {
            D3D12_PLACED_SUBRESOURCE_FOOTPRINT footprint;
            Device->Device.Get()->GetCopyableFootprints(&resourceDesc, 0, 1, 0, &footprint, null, null, null);

            D3D12_TEXTURE_COPY_LOCATION copyDst = new()
            {
                Type = D3D12_TEXTURE_COPY_TYPE.D3D12_TEXTURE_COPY_TYPE_SUBRESOURCE_INDEX,
                pResource = destination,
                SubresourceIndex = 0
            };
            D3D12_TEXTURE_COPY_LOCATION copySrc = new()
            {
                Type = D3D12_TEXTURE_COPY_TYPE.D3D12_TEXTURE_COPY_TYPE_PLACED_FOOTPRINT,
                pResource = tempBuffer,
                PlacedFootprint = footprint,
                SubresourceIndex = 0
            };
            commandList->CopyTextureRegion(&copyDst, 0, 0, 0, &copySrc, null);
        }

        // CLose the command list
        commandList->Close();

        // Execute the command list on the queue and reset the state.
        {
            var lockToken = false;
            QueueLock.Enter(ref lockToken);
            Debug.Assert(lockToken);

            var queue = CommandQueue.Get();
            var fence = Fence.Get();

            queue->ExecuteCommandLists(1, (ID3D12CommandList**)&commandList);
            var fenceValue = ++FenceValue;
            queue->Signal(Fence, fenceValue);
            if (fence->GetCompletedValue() < fenceValue)
            {
                fence->SetEventOnCompletion(FenceValue, FenceEvent);
                Kernel32.WaitForSingleObject(FenceEvent, Win32Common.INFINITE);
            }
            frame->State = UploadState.Available;
            QueueLock.Exit();
        }
        return true;
    }

    private UploadFrame* GetAvailableFrame()
    {
        var wait = new SpinWait();
        while (true)
        {
            var lockToken = false;
            var index = -1;
            FrameLock.Enter(ref lockToken);
            Debug.Assert(lockToken);

            for (var i = 0; i < UploadFrames.Size; ++i)
            {
                if (UploadFrames[i].State == UploadState.Available)
                {
                    UploadFrames[i].State = UploadState.Busy;
                    index = i;
                    break;
                }
            }
            FrameLock.Exit();
            if (index != -1)
            {
                return UploadFrames.AsPointer() + index;
            }
            wait.SpinOnce();
        }
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(D3D12UploadQueue* queue)
    {
        foreach (ref var frame in queue->UploadFrames)
        {
            //NOTE(Jens): We might have to implement some kind of wait here.
            frame.CommandList.Dispose();
            frame.Allocator.Dispose();
        }
        queue->CommandQueue.Dispose();
        queue->Fence.Dispose();
        if (queue->FenceEvent.IsValid())
        {
            Kernel32.CloseHandle(queue->FenceEvent);
        }

        *queue = default;
    }

}
