using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Upload;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics;

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

public record struct CreateDepthBufferArgs
{
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public float ClearValue { get; init; }
}

public ref struct CreateRootSignatureArgs
{
    public uint NumberOfDescriptors;
    public required ReadOnlySpan<uint> Parameters;
    public required ReadOnlySpan<(SamplerState State, ShaderVisibility Visibility)> Samplers;
}

public enum ShaderVisibility
{
    All,
    Pixel,
    Vertex
}

[UnmanagedResource]
public unsafe partial struct D3D12ResourceManager
{
    private ResourcePool<D3D12Buffer> _buffers;
    private ResourcePool<D3D12Texture> _textures;
    private ResourcePool<D3D12RootSignature> _rootSignatures;
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

        if (!memoryManager.TryCreateResourcePool(out manager->_rootSignatures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12RootSignature)} Count = {count}.");
        }
    }

    [System(SystemStage.PostShutdown)]
    internal static void Shutdown(D3D12ResourceManager* manager, IMemoryManager memoryManager)
    {
        memoryManager.FreeResourcePool(ref manager->_buffers);
        memoryManager.FreeResourcePool(ref manager->_textures);
        memoryManager.FreeResourcePool(ref manager->_rootSignatures);
    }

    public readonly Handle<Buffer> CreateBuffer(in CreateBufferArgs args)
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

    public readonly Handle<Texture> CreateDepthBuffer(in CreateDepthBufferArgs args)
    {
        var handle = _textures.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a slot for the texture");
            return Handle<Texture>.Invalid;
        }

        var texture = _textures.AsPtr(handle);
        texture->Resource = _device->CreateDepthBuffer(args.Width, args.Height);
        if (!texture->Resource.IsValid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to create the DepthBuffer.");
            _textures.SafeFree(handle);
            return Handle<Texture>.Invalid;
        }

        texture->DSV = _allocator->Allocate(DescriptorHeapType.DepthStencilView);
        if (!texture->DSV.IsValid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a Depth Stencil descriptor.");
            texture->Resource.Dispose();
            _textures.SafeFree(handle);
            return Handle<Texture>.Invalid;
        }

        _device->CreateDepthStencilView(texture->Resource, texture->DSV.CPU);


        return handle.Value;
    }

    public readonly Handle<Texture> CreateTexture(in CreateTextureArgs args)
        => CreateTexture(args, null);

    /// <summary>
    /// Internal texture creation used for the Backbuffer
    /// </summary>
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
            var flags = args.RenderTargetView ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;
            texture->Resource = _device->CreateTexture(args.Width, args.Height, args.Format, flags);
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

    public readonly void DestroyTexture(Handle<Texture> handle)
    {
        Debug.Assert(handle.IsValid);
        var texture = _textures.AsPtr(handle.Value);
        texture->Resource.Dispose();

        FreeDescriptor(texture->RTV, _allocator);
        FreeDescriptor(texture->SRV, _allocator);
        FreeDescriptor(texture->DSV, _allocator);

        *texture = default;
        _textures.SafeFree(handle.Value);

        static void FreeDescriptor(in DescriptorHandle descriptor, D3D12Allocator* allocator)
        {
            if (descriptor.IsValid)
            {
                allocator->Free(descriptor);
            }
        }
    }

    public readonly Handle<RootSignature> CreateRootSignature(in CreateRootSignatureArgs args)
    {
        var handle = _rootSignatures.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a slot for the root signature.");
            return Handle<RootSignature>.Invalid;
        }

        var rootSignature = _rootSignatures.AsPtr(handle);




        TitanList<D3D12_ROOT_PARAMETER1> parameters = stackalloc D3D12_ROOT_PARAMETER1[10];
        TitanList<D3D12_STATIC_SAMPLER_DESC> samplers = stackalloc D3D12_STATIC_SAMPLER_DESC[args.Samplers.Length];

        if (args.NumberOfDescriptors > 0)
        {
            Span<D3D12_DESCRIPTOR_RANGE1> ranges = stackalloc D3D12_DESCRIPTOR_RANGE1[(int)args.NumberOfDescriptors];
            D3D12Helpers.InitDescriptorRanges(ranges, D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
            parameters.Add(CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(ranges));
        }

        if (args.Samplers.Length > 0)
        {
            for (var i = 0; i < args.Samplers.Length; ++i)
            {
                //NOTE(Jens): We can improve this by specifying the visibility of the static sampler.
                var visibility = args.Samplers[i].Visibility switch
                {
                    ShaderVisibility.Pixel => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL,
                    ShaderVisibility.Vertex => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_VERTEX,
                    _ => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_ALL
                };
                samplers.Add(D3D12Helpers.CreateStaticSamplerDesc(args.Samplers[i].State, (uint)i, 0, visibility));
            }
        }
        var flags = D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_NONE;
        rootSignature->Resource = _device->CreateRootSignature(flags, parameters, samplers);
        if (!rootSignature->Resource.IsValid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the {nameof(ID3D12RootSignature)}.");
            _rootSignatures.SafeFree(handle);
            return Handle<RootSignature>.Invalid;
        }

        return handle.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RootSignature* Access(Handle<RootSignature> rootSignature)
    {
        Debug.Assert(rootSignature.IsValid, "Trying to access a resource of an invalid handle");
        return (RootSignature*)_rootSignatures.AsPtr(rootSignature.Value);
    }

    public void DestroyRootSignature(Handle<RootSignature> handle)
    {
        Debug.Assert(handle.IsValid);
        var rootSignature = _rootSignatures.AsPtr(handle.Value);
        rootSignature->Resource.Dispose();
        *rootSignature = default;
        _rootSignatures.SafeFree(handle.Value);
    }
}
