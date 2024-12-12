using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Upload;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.D3D12;

public record struct CreateBufferArgs(uint Count, int Stride, BufferType Type, TitanBuffer InitialData = default)
{
    public bool CpuVisible { get; init; }
    public bool ShaderVisible { get; init; }

    public static unsafe CreateBufferArgs Create<T>(uint count, BufferType type, TitanBuffer initialData = default, bool cpuVisible = false, bool shaderVisible = false) where T : unmanaged
        => new(count, sizeof(T), type, initialData)
        {
            CpuVisible = cpuVisible,
            ShaderVisible = shaderVisible
        };
}

public record struct CreateTextureArgs
{
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public required DXGI_FORMAT Format { get; init; }
    public TitanBuffer InitialData { get; init; }
    public bool ShaderVisible { get; init; } // maybe we want specific shader visibility?
    public bool RenderTargetView { get; init; }
    public Color OptimizedClearColor { get; init; }
    public string? DebugName { get; init; }
}

public record struct CreateDepthBufferArgs
{
    public required DXGI_FORMAT Format { get; init; }
    public required uint Width { get; init; }
    public required uint Height { get; init; }
    public float ClearValue { get; init; }
}


public ref struct DepthStencilArgs
{
    public bool DepthEnabled { get; init; }
    public bool StencilEnabled { get; init; }
    public DXGI_FORMAT Format { get; init; }
}
public ref struct CreatePipelineStateArgs
{
    public required Handle<RootSignature> RootSignature { get; init; }
    public TitanBuffer PixelShader { get; init; }
    public TitanBuffer VertexShader { get; init; }
    public D3D12_PRIMITIVE_TOPOLOGY_TYPE Topology { get; init; }
    public required ReadOnlySpan<Handle<Texture>> RenderTargets { get; init; }
    public DepthStencilArgs Depth { get; init; }
    public BlendStateType BlendState { get; init; }
}

public ref struct CreateRootSignatureArgs
{
    public required ReadOnlySpan<RootSignatureParameter> Parameters;
}

public enum RootSignatureParameterType : byte
{
    Constant,
    ConstantBuffer,
    DescriptorRange,
    Sampler
}

[StructLayout(LayoutKind.Explicit)]
public struct RootSignatureParameter
{
    [FieldOffset(0)]
    public RootSignatureParameterType Type;

    [FieldOffset(1)]
    public byte Register;

    [FieldOffset(2)]
    public byte Space;

    [FieldOffset(3)]
    public ShaderVisibility Visibility;

    [FieldOffset(3)]
    public ShaderDescriptorRangeType RangeType;

    [FieldOffset(4)]
    public byte Count;

    [FieldOffset(4)]
    public ConstantBufferFlags ConstantBufferFlags;

    [FieldOffset(4)]
    public SamplerState SamplerState;
}

public enum ShaderVisibility : byte
{
    All,
    Pixel,
    Vertex
}

public enum ConstantBufferFlags : byte
{
    None,
    Static,
    Volatile
}

public readonly unsafe struct MappedGPUResource<T>(T* resource, Handle<GPUBuffer> handle, uint count) where T : unmanaged
{
    public T* Ptr => resource;
    public Handle<GPUBuffer> Handle => handle;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingle(in T value, uint offset = 0) => *(resource + offset) = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<T> values, uint offset = 0)
    {
        if (values.Length == 0)
        {
            return;
        }
        Debug.Assert(values.Length + offset < count);
        MemoryUtils.Copy(resource + offset, values);
    }
}

[UnmanagedResource]
public unsafe partial struct D3D12ResourceManager
{
    private ResourcePool<GPUBuffer> _buffers;
    private ResourcePool<Texture> _textures;
    private ResourcePool<RootSignature> _rootSignatures;
    private ResourcePool<PipelineState> _pipelineStates;
    private D3D12Device* _device;
    private D3D12UploadQueue* _uploadQueue;
    private D3D12Allocator* _allocator;

    [System(SystemStage.Startup)]
    internal static void Startup(D3D12ResourceManager* manager, IMemoryManager memoryManager, UnmanagedResourceRegistry registry)
    {
        var count = 1024u;
        if (!memoryManager.TryCreateResourcePool(out manager->_buffers, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(GPUBuffer)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_textures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(Texture)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_rootSignatures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(RootSignature)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_pipelineStates, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(PipelineState)} Count = {count}.");
            return;
        }


        manager->_device = registry.GetResourcePointer<D3D12Device>();
        manager->_uploadQueue = registry.GetResourcePointer<D3D12UploadQueue>();
        manager->_allocator = registry.GetResourcePointer<D3D12Allocator>();
    }

    [System(SystemStage.EndOfLife)]
    internal static void Shutdown(D3D12ResourceManager* manager, IMemoryManager memoryManager)
    {
        memoryManager.FreeResourcePool(ref manager->_buffers);
        memoryManager.FreeResourcePool(ref manager->_textures);
        memoryManager.FreeResourcePool(ref manager->_rootSignatures);
        memoryManager.FreeResourcePool(ref manager->_pipelineStates);
    }

    public readonly Handle<GPUBuffer> CreateBuffer(in CreateBufferArgs args)
    {
        Debug.Assert(args.Count > 0);
        Debug.Assert(args.Stride > 0);

        Debug.Assert(args.Type != BufferType.Index || args.Stride is 2 or 4);
        Debug.Assert(args.Type != BufferType.Constant || (args.Stride * args.Count) % 256 == 0, "Constant buffers must be a multiple of 256");
        Debug.Assert(args.Type != BufferType.Structured || (args.Stride % 16) == 0, "A structured buffer must be 16 byte aligned.");

        var handle = _buffers.SafeAlloc();
        if (handle.IsInvalid)
        {
            return Handle<GPUBuffer>.Invalid;
        }
        var buffer = _buffers.AsPtr(handle);

        //NOTE(Jens): Creating a buffer like this is not very good in a bindless renderer. 

        //var resourceState = args.Type switch
        //{
        //    BufferType.Index => D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_INDEX_BUFFER,
        //    BufferType.Vertex or BufferType.Constant or _ => D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER,
        //};

        //if (args.InitialData.IsValid)
        //{
        //    resourceState = D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COPY_DEST;
        //}

        //if (args.InitialData.IsValid)
        //{

        //}
        var resourceState = D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;
        var size = (uint)args.Stride * args.Count;
        var flags = args.ShaderVisible
            ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE
            : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;

        buffer->Resource = _device->CreateBuffer(size, args.CpuVisible, resourceState, flags);
        if (!buffer->Resource.IsValid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the {nameof(ID3D12Resource)}.");
            _buffers.SafeFree(handle);
            return Handle<GPUBuffer>.Invalid;
        }

        buffer->StartOffset = 0;
        buffer->Stride = (uint)args.Stride;
        buffer->Count = args.Count;
        buffer->Type = args.Type;

        if (args.ShaderVisible)
        {
            buffer->SRV = _allocator->Allocate(DescriptorHeapType.ShaderResourceView);
            if (!buffer->SRV.IsValid)
            {
                Logger.Error<D3D12ResourceManager>("Failed to allocate the SRV for buffer.");
                buffer->Resource.Dispose();
                _buffers.SafeFree(handle);
                return Handle<GPUBuffer>.Invalid;
            }

            //TODO(Jens): Revisit this code. Creating the ConstantBUfferView instead of ShaderResourceView makes the CB non accessible in the shader 1 out of 10 tries..
            switch (args.Type)
            {
                case BufferType.Constant:
                    _device->CreateConstantBufferView(buffer->Size, buffer->Resource.Get()->GetGPUVirtualAddress(), buffer->SRV.CPU);
                    break;
                case BufferType.Structured:
                case BufferType.Index:
                case BufferType.Vertex:
                    _device->CreateShaderResourceView1(buffer->Resource, buffer->SRV.CPU, buffer->Count, buffer->Stride);
                    break;
            }
        }

        if (!args.InitialData.IsValid)
        {
            return handle;
        }

        Debug.Assert(args.InitialData.Size <= size, "The data size is greater than the buffer size.");

        Logger.Trace<D3D12ResourceManager>($"Buffer has initial data, trying to upload. Size = {args.InitialData.Size}");

        if (!_uploadQueue->Upload(buffer->Resource, args.InitialData))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to upload data to the buffer. Size = {size} Data Size = {args.InitialData.Size}");
            buffer->Resource.Dispose();
            _buffers.SafeFree(handle);
            return Handle<GPUBuffer>.Invalid;
        }

        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly GPUBuffer* Access(Handle<GPUBuffer> handle)
    {
        Debug.Assert(handle.IsValid, "Trying to access a resource of an invalid handle");
        return _buffers.AsPtr(handle);
    }

    public readonly void DestroyBuffer(Handle<GPUBuffer> handle)
    {
        Debug.Assert(handle.IsValid, "Trying to destroy a handle that is invalid.");

        var buffer = _buffers.AsPtr(handle);
        buffer->Resource.Dispose();
        *buffer = default;

        _buffers.SafeFree(handle);
    }

    public readonly bool TryMapBuffer<T>(in Handle<GPUBuffer> handle, out MappedGPUResource<T> resource) where T : unmanaged
    {
        var buffer = Access(handle);
        Debug.Assert(buffer->Resource.IsValid);
        T* data;
        var result = buffer->Resource.Get()->Map(0, null, (void**)&data);
        if (Win32Common.SUCCEEDED(result))
        {
            resource = new MappedGPUResource<T>(data, handle, buffer->Count);
            return true;
        }
        resource = default;
        return false;
    }

    public readonly void Unmap<T>(in MappedGPUResource<T> resource) where T : unmanaged
    {
        var buffer = Access(resource.Handle);
        Debug.Assert(buffer->Resource.IsValid);
        buffer->Resource.Get()->Unmap(0, null);
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
        *texture = default;

        texture->Resource = _device->CreateDepthBuffer(args.Width, args.Height, args.Format);
        texture->Width = args.Width;
        texture->Height = args.Height;
        texture->Format = args.Format;
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

        return handle;
    }

    public readonly Handle<Texture> CreateTextureHandle()
    {
        var handle = _textures.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a slot for the texture.");
            return Handle<Texture>.Invalid;
        }

        return handle;
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
        *texture = default;
        if (backbuffer == null)
        {
            if (args.RenderTargetView)
            {
                var clearValue = D3D12Helpers.ClearColor(args.Format, args.OptimizedClearColor);
                texture->Resource = _device->CreateTexture(args.Width, args.Height, args.Format, D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET, &clearValue);
            }
            else
            {
                texture->Resource = _device->CreateTexture(args.Width, args.Height, args.Format);
            }

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

        texture->Width = args.Width;
        texture->Height = args.Height;
        texture->Format = args.Format;

        if (args.DebugName != null)
        {
            D3D12Helpers.SetName(texture->Resource, args.DebugName);
        }
        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Texture* Access(in Handle<Texture> texture)
    {
        Debug.Assert(texture.IsValid, "Trying to access a resource of an invalid handle");
        return _textures.AsPtr(texture);
    }

    public readonly void DestroyTexture(Handle<Texture> handle)
    {
        Debug.Assert(handle.IsValid);
        var texture = _textures.AsPtr(handle);
        texture->Resource.Dispose();

        FreeDescriptor(texture->RTV, _allocator);
        FreeDescriptor(texture->SRV, _allocator);
        FreeDescriptor(texture->DSV, _allocator);

        *texture = default;
        _textures.SafeFree(handle);

        static void FreeDescriptor(in D3D12DescriptorHandle descriptor, D3D12Allocator* allocator)
        {
            if (descriptor.IsValid)
            {
                allocator->Free(descriptor);
            }
        }
    }


    public readonly bool Upload(Handle<Texture> handle, TitanBuffer buffer)
    {
        var texture = Access(handle);
        return _uploadQueue->Upload(texture->Resource, buffer);
    }


    public readonly bool Upload(Handle<GPUBuffer> handle, TitanBuffer data, uint offset = 0)
    {
        var buffer = Access(handle);
        return _uploadQueue->Upload(buffer->Resource, data, offset);
    }

    /// <summary>
    /// Creates a new root signature with ranges, constants, constant buffers and static samplers.
    /// <remarks>
    /// <para>The order of the root parameters are:</para>
    /// 1. Descriptor Ranges<br/>
    /// 2. ConstantBufferViews<br/>
    /// 3. Constants<br/>
    /// </remarks>
    /// </summary>
    /// <returns>The handle to the root signature, or Invalid on failure</returns>
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
        TitanList<D3D12_STATIC_SAMPLER_DESC> samplers = stackalloc D3D12_STATIC_SAMPLER_DESC[10];

        var tempBufferSize = sizeof(D3D12_DESCRIPTOR_RANGE1) * 16;
        var tempBuffer = stackalloc byte[tempBufferSize];
        var tempAllocator = new BumpAllocator(tempBuffer, (uint)tempBufferSize);

        Logger.Trace<D3D12ResourceManager>($"Creating root signature. {args.Parameters.Length} {tempBufferSize} bytes");
        foreach (ref readonly var parameter in args.Parameters)
        {
            switch (parameter.Type)
            {
                case RootSignatureParameterType.Constant:
                    parameters.Add(CD3DX12_ROOT_PARAMETER1.AsConstants(parameter.Count, parameter.Register, parameter.Space, ToD3D12ShaderVisibility(parameter.Visibility)));
                    break;
                case RootSignatureParameterType.Sampler:
                    samplers.Add(D3D12Helpers.CreateStaticSamplerDesc(parameter.SamplerState, parameter.Register, parameter.Space, ToD3D12ShaderVisibility(parameter.Visibility)));
                    break;
                case RootSignatureParameterType.ConstantBuffer:
                    var constantBufferFlags = parameter.ConstantBufferFlags switch
                    {
                        ConstantBufferFlags.Volatile => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_VOLATILE,
                        ConstantBufferFlags.Static => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_STATIC,
                        _ => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_NONE
                    };
                    parameters.Add(CD3DX12_ROOT_PARAMETER1.AsConstantBufferView(parameter.Register, parameter.Space, constantBufferFlags, ToD3D12ShaderVisibility(parameter.Visibility)));
                    break;
                case RootSignatureParameterType.DescriptorRange:
                    var type = parameter.RangeType switch
                    {
                        ShaderDescriptorRangeType.ShaderResourceView => D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV,
                        _ => D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV
                    };
                    Debug.Assert(parameter.Count > 0);
                    var ranges = tempAllocator.AllocateArray<D3D12_DESCRIPTOR_RANGE1>(parameter.Count);
                    D3D12Helpers.InitDescriptorRanges(ranges, type, parameter.Register, parameter.Space);

                    parameters.Add(CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(ranges));

                    break;
            }
        }

        // not sure what to use the flags for yet.
        var flags = D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_NONE;
        rootSignature->Resource = _device->CreateRootSignature(flags, parameters, samplers);
        if (!rootSignature->Resource.IsValid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the {nameof(ID3D12RootSignature)}.");
            _rootSignatures.SafeFree(handle);
            return Handle<RootSignature>.Invalid;
        }

        return handle;


        D3D12_SHADER_VISIBILITY ToD3D12ShaderVisibility(ShaderVisibility visibility)
            => visibility switch
            {
                ShaderVisibility.Pixel => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL,
                ShaderVisibility.Vertex => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_VERTEX,
                _ => D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_ALL
            };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly RootSignature* Access(Handle<RootSignature> rootSignature)
    {
        Debug.Assert(rootSignature.IsValid, "Trying to access a resource of an invalid handle");
        return _rootSignatures.AsPtr(rootSignature);
    }

    public void DestroyRootSignature(Handle<RootSignature> handle)
    {
        Debug.Assert(handle.IsValid);
        var rootSignature = _rootSignatures.AsPtr(handle);
        rootSignature->Resource.Dispose();
        *rootSignature = default;
        _rootSignatures.SafeFree(handle);
    }

    public readonly Handle<PipelineState> CreatePipelineState(in CreatePipelineStateArgs args)
    {
        var handle = _pipelineStates.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to allocate a slot for the {nameof(PipelineState)}");
            return Handle<PipelineState>.Invalid;
        }

        Debug.Assert(args.RootSignature.IsValid);
        var rootSignature = _rootSignatures.AsPtr(args.RootSignature);
        Debug.Assert(rootSignature->Resource.IsValid);

        TitanList<DXGI_FORMAT> formats = stackalloc DXGI_FORMAT[10];
        foreach (var textureHandle in args.RenderTargets)
        {
            Debug.Assert(textureHandle.IsValid);
            var texture = _textures.AsPtr(textureHandle);
            Debug.Assert(texture != null);
            formats.Add(texture->Format);
        }

        var pipelineState = _pipelineStates.AsPtr(handle);

        var psoStream = new D3D12PipelineSubobjectStream()
                .Blend(D3D12Helpers.GetBlendState(args.BlendState)) //TODO(Jens): Should be configurable, but keep it simple for now.
                .Topology(args.Topology)
                .Razterizer(D3D12_RASTERIZER_DESC.Default() with
                {
                    CullMode = D3D12_CULL_MODE.D3D12_CULL_MODE_NONE, // TODO(Jens): Should be configurable
                })
                .RenderTargetFormat(new(formats.AsReadOnlySpan()))
                .RootSignature(rootSignature->Resource)
                .Sample(new DXGI_SAMPLE_DESC
                {
                    Count = 1,
                    Quality = 0
                })
                .SampleMask(uint.MaxValue)
            ;

        if (args.VertexShader.IsValid)
        {
            psoStream = psoStream.VS(new()
            {
                BytecodeLength = args.VertexShader.Size,
                pShaderBytecode = args.VertexShader.AsPointer()
            });
        }

        if (args.PixelShader.IsValid)
        {
            psoStream = psoStream.PS(new()
            {
                BytecodeLength = args.PixelShader.Size,
                pShaderBytecode = args.PixelShader.AsPointer()
            });
        }
        if (args.Depth.DepthEnabled)
        {
            D3D12_DEPTH_STENCIL_DESC depthStencilDesc = new()
            {
                DepthEnable = 1,
                DepthWriteMask = D3D12_DEPTH_WRITE_MASK.D3D12_DEPTH_WRITE_MASK_ALL,
                DepthFunc = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_LESS,
                StencilEnable = args.Depth.StencilEnabled ? 1 : 0
            };
            psoStream = psoStream
                .DepthStencil(depthStencilDesc)
                .DepthStencilfFormat(args.Depth.Format);
        }

        pipelineState->Resource = _device->CreatePipelineStateObject(psoStream.AsStreamDesc());

        if (pipelineState == null)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the {nameof(ID3D12PipelineState)}.");
            _pipelineStates.SafeFree(handle);
            return default;
        }

        return handle;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PipelineState* Access(in Handle<PipelineState> handle)
        => _pipelineStates.AsPtr(handle);

    public void DestroyPipelineState(Handle<PipelineState> handle)
    {
        Debug.Assert(handle.IsValid);
        var pipeline = _pipelineStates.AsPtr(handle);
        pipeline->Resource.Dispose();
        *pipeline = default;
        _pipelineStates.SafeFree(handle);
    }
}


