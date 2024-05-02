using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Upload;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics;

public enum BufferType
{
    Vertex,
    Index,
    Constant
}

public struct Buffer
{
    public uint Count;
    public uint Stride;
    public BufferType Type;
    public uint Size => Count * Stride;
}

public struct D3D12Buffer
{
    public Buffer Buffer;
    public ComPtr<ID3D12Resource> Resource;
    public uint StartOffset;
}

public record struct CreateBufferArgs(uint Count, int Stride, BufferType Type, TitanBuffer InitialData = default)
{
    public bool CpuVisible { get; init; }
}

[UnmanagedResource]
public unsafe partial struct D3D12ResourceManager
{
    private ResourcePool<D3D12Buffer> _buffers;
    private D3D12Device* _device;
    private D3D12UploadQueue* _uploadQueue;

    [System(SystemStage.Init)]
    internal static void Init(D3D12ResourceManager* manager, IMemoryManager memoryManager, in D3D12Device device, in D3D12UploadQueue uploadQueue)
    {
        var count = 1024u;
        manager->_device = MemoryUtils.AsPointer(device);
        manager->_uploadQueue = MemoryUtils.AsPointer(uploadQueue);
        if (!memoryManager.TryCreateResourcePool(out manager->_buffers, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12Buffer)} Count = {count}.");
        }
    }

    [System(SystemStage.Shutdown)]
    internal static void Shutdown(D3D12ResourceManager* manager, IMemoryManager memoryManager)
    {
        memoryManager.FreeResourcePool(ref manager->_buffers);
    }

    public readonly Handle<Buffer> CreateBuffer(CreateBufferArgs args)
    {
        Debug.Assert(args.Count > 0);
        Debug.Assert(args.Stride > 0);

        Debug.Assert(args.Type != BufferType.Index || args.Stride is 2 or 4);

        var handle = _buffers.SafeAlloc();
        if (handle.IsInvalid)
        {
            return Handle<Buffer>.Invalid;
        }
        var buffer = _buffers.AsPtr(handle);

        //NOTE(Jens): Creating a buffer like this is not very good in a bindless renderer. 

        var resourceState = args.Type switch
        {
            BufferType.Index => D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_INDEX_BUFFER,
            BufferType.Vertex or BufferType.Constant or _ => D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER,
        };

        var size = (uint)args.Stride * args.Count;

        buffer->Resource = _device->CreateBuffer(size, true, resourceState);
        if (!buffer->Resource.IsValid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the {nameof(ID3D12Resource)}.");
            _buffers.SafeFree(handle);
            return Handle<Buffer>.Invalid;
        }

        buffer->StartOffset = 0;
        buffer->Buffer.Stride = (uint)args.Stride;
        buffer->Buffer.Count = args.Count;
        buffer->Buffer.Type = args.Type;

        if (!args.InitialData.IsValid)
        {
            return handle.Value;
        }

        Debug.Assert(args.InitialData.Size <= size, "The data size is greater than the buffer size.");
        
        Logger.Trace<D3D12ResourceManager>($"Buffer has initial data, trying to upload. Size = {args.InitialData.Size}");

        if (!_uploadQueue->Upload(buffer->Resource, args.InitialData))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to upload data to the buffer. Size = {size} Data Size = {args.InitialData.Size}");
            buffer->Resource.Dispose();
            _buffers.SafeFree(handle);
            return Handle<Buffer>.Invalid;
        }

        return handle.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Buffer* Access(Handle<Buffer> handle)
    {
        Debug.Assert(handle.IsValid, "Trying to access a resource of an invalid handle");
        return (Buffer*)_buffers.AsPtr(handle.Value);
    }

    public readonly void DestroyBuffer(Handle<Buffer> handle)
    {
        Debug.Assert(handle.IsValid, "Trying to destroy a handle that is invalid.");

        var buffer = _buffers.AsPtr(handle.Value);
        buffer->Resource.Dispose();
        *buffer = default;

        _buffers.SafeFree(handle.Value);
    }
}
