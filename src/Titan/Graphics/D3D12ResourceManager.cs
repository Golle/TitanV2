using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Upload;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics;

public struct Texture
{
    public uint Width;
    public uint Height;
}

internal struct D3D12Texture
{
    public Texture Texture;
    public ComPtr<ID3D12Resource> Resource;
    public DescriptorHandle SRV;
    public DescriptorHandle RTV;
}

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

public record struct CreateTextureArgs
{
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public required DXGI_FORMAT Format { get; init; }
    public TitanBuffer InitialData { get; init; }
    public bool ShaderVisible { get; init; } // maybe we want specific shader visibility?
    public bool RenderTargetView { get; init; }
}

[UnmanagedResource]
public unsafe partial struct D3D12ResourceManager
{
    private ResourcePool<D3D12Buffer> _buffers;
    private ResourcePool<D3D12Texture> _textures;
    private D3D12Device* _device;
    private D3D12UploadQueue* _uploadQueue;
    private D3D12Allocator* _allocator;

    [System(SystemStage.Init)]
    internal static void Init(D3D12ResourceManager* manager, IMemoryManager memoryManager, in D3D12Device device, in D3D12UploadQueue uploadQueue, in D3D12Allocator allocator)
    {
        var count = 1024u;
        manager->_device = MemoryUtils.AsPointer(device);
        manager->_uploadQueue = MemoryUtils.AsPointer(uploadQueue);
        manager->_allocator = MemoryUtils.AsPointer(allocator);

        if (!memoryManager.TryCreateResourcePool(out manager->_buffers, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12Buffer)} Count = {count}.");
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_textures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12Texture)} Count = {count}.");
        }
    }

    [System(SystemStage.Shutdown)]
    internal static void Shutdown(D3D12ResourceManager* manager, IMemoryManager memoryManager)
    {
        memoryManager.FreeResourcePool(ref manager->_buffers);
        memoryManager.FreeResourcePool(ref manager->_textures);
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


    public readonly Handle<Texture> CreateTexture(in CreateTextureArgs args)
        => CreateTexture(args, null);

    internal readonly Handle<Texture> CreateTexture(in CreateTextureArgs args, ID3D12Resource* backbuffer)
    {
        var handle = _textures.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a slot for the texture.");
            return Handle<Texture>.Invalid;
        }

        var texture = _textures.AsPtr(handle);
        if (backbuffer == null)
        {
            texture->Resource = _device->CreateTexture(args.Width, args.Height, args.Format);
            if (!texture->Resource.IsValid)
            {
                Logger.Error<D3D12ResourceManager>("Failed to create the underlying resource for the texture.");
                _textures.SafeFree(handle);
                return Handle<Texture>.Invalid;
            }

            if (args.InitialData.IsValid && !_uploadQueue->Upload(texture->Resource, args.InitialData))
            {
                Logger.Error<D3D12ResourceManager>("Upload data to the texture failed, destroying the resource.");
                texture->Resource.Dispose();
                return Handle<Texture>.Invalid;
            }

            if (args.ShaderVisible)
            {
                texture->SRV = _allocator->Allocate(DescriptorHeapType.ShaderResourceView);
                if (!texture->SRV.IsValid)
                {
                    Logger.Error<D3D12ResourceManager>("Failed to allocate a SRV descriptor handle");
                    texture->Resource.Dispose();
                    _textures.SafeFree(handle);
                    return Handle<Texture>.Invalid;
                }
                _device->CreateShaderResourceView(texture->Resource, args.Format, texture->SRV.CPU);
            }
        }
        else
        {
            texture->Resource = backbuffer;
        }


        if (args.RenderTargetView)
        {
            texture->RTV = _allocator->Allocate(DescriptorHeapType.RenderTargetView);
            if (!texture->RTV.IsValid)
            {
                Logger.Error<D3D12ResourceManager>("Failed to allocate a RTV descriptor handle");
                if (texture->RTV.IsValid)
                {
                    _allocator->Free(texture->RTV);
                }
                texture->Resource.Dispose();
                _textures.SafeFree(handle);
                return Handle<Texture>.Invalid;
            }

            _device->CreateRenderTargetView(texture->Resource, null, texture->RTV.CPU);
        }

        texture->Texture.Width = args.Width;
        texture->Texture.Height = args.Height;

        return handle.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Texture* Access(Handle<Texture> texture)
    {
        Debug.Assert(texture.IsValid, "Trying to access a resource of an invalid handle");
        return (Texture*)_textures.AsPtr(texture.Value);
    }

    public readonly void DestroyTexture(Handle<Texture> texture)
    {

    }

}
