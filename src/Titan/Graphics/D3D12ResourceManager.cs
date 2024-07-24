using Microsoft.Win32;
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
using Titan.Graphics.Resources;
using Titan.Platform.Win32;
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
}

public ref struct CreateRootSignatureArgs
{
    public required ReadOnlySpan<ConstantsInfo> Constants;
    public required ReadOnlySpan<ConstantBufferInfo> ConstantBuffers;
    public required ReadOnlySpan<DescriptorRangesInfo> Ranges;
    public required ReadOnlySpan<SamplerInfo> Samplers;
}

public struct ConstantsInfo
{
    public byte Count;
    public ShaderVisibility Visibility;
    public byte Register;
    public byte Space;
}
public struct ConstantBufferInfo
{
    public ConstantBufferFlags Flags;
    public ShaderVisibility Visibility;
    public byte Register;
    public byte Space;
}
public struct DescriptorRangesInfo
{
    public byte Count;
    public ShaderDescriptorRangeType Type;
    public byte Register;
    public byte Space;
}

public struct SamplerInfo
{
    public SamplerState State;
    public ShaderVisibility Visibility;
    public byte Register;
    public byte Space;
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

[UnmanagedResource]
public unsafe partial struct D3D12ResourceManager
{
    private ResourcePool<D3D12Buffer> _buffers;
    private ResourcePool<D3D12Texture> _textures;
    private ResourcePool<D3D12RootSignature> _rootSignatures;
    private ResourcePool<D3D12PipelineState> _pipelineStates;
    private D3D12Device* _device;
    private D3D12UploadQueue* _uploadQueue;
    private D3D12Allocator* _allocator;

    [System(SystemStage.Startup)]
    internal static void Startup(D3D12ResourceManager* manager, IMemoryManager memoryManager, UnmanagedResourceRegistry registry)
    {
        var count = 1024u;
        if (!memoryManager.TryCreateResourcePool(out manager->_buffers, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12Buffer)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_textures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12Texture)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_rootSignatures, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12RootSignature)} Count = {count}.");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out manager->_pipelineStates, count))
        {
            Logger.Error<D3D12ResourceManager>($"Failed to create the resource pool. Resource = {nameof(D3D12PipelineState)} Count = {count}.");
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

    public readonly Handle<Texture> CreateTextureHandle()
    {
        var handle = _textures.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>("Failed to allocate a slot for the texture.");
            return Handle<Texture>.Invalid;
        }

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
        texture->Format = args.Format;

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
        TitanList<D3D12_STATIC_SAMPLER_DESC> samplers = stackalloc D3D12_STATIC_SAMPLER_DESC[args.Samplers.Length];
        Span<D3D12_DESCRIPTOR_RANGE1> rangeDescriptor = stackalloc D3D12_DESCRIPTOR_RANGE1[10];

        // Set up the descriptor ranges
        foreach (var range in args.Ranges)
        {
            var type = range.Type switch
            {
                ShaderDescriptorRangeType.ShaderResourceView => D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV,
                _ => D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV
            };
            var ranges = rangeDescriptor[..range.Count];
            D3D12Helpers.InitDescriptorRanges(ranges, type, range.Register, range.Space);
            
            //TODO(Jens): Add visibility if we need to.
            parameters.Add(CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(ranges));
        }

        foreach (var constantBuffer in args.ConstantBuffers)
        {
            var constantBufferFlags = constantBuffer.Flags switch
            {
                ConstantBufferFlags.Volatile => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_VOLATILE,
                ConstantBufferFlags.Static => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_STATIC,
                _ => D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_NONE
            };
            parameters.Add(CD3DX12_ROOT_PARAMETER1.AsConstantBufferView(constantBuffer.Register, constantBuffer.Space, constantBufferFlags, ToD3D12ShaderVisibility(constantBuffer.Visibility)));
        }

        foreach (var constant in args.Constants)
        {
            parameters.Add(CD3DX12_ROOT_PARAMETER1.AsConstants(constant.Count, constant.Register, constant.Space, ToD3D12ShaderVisibility(constant.Visibility)));
        }

        // Set up the samplers
        if (args.Samplers.Length > 0)
        {
            for (var i = 0; i < args.Samplers.Length; ++i)
            {
                //NOTE(Jens): We can improve this by specifying the visibility of the static sampler.
                var visibility = ToD3D12ShaderVisibility(args.Samplers[i].Visibility);
                samplers.Add(D3D12Helpers.CreateStaticSamplerDesc(args.Samplers[i].State, (uint)i, 0, visibility));
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

        return handle.Value;


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

    public readonly Handle<PipelineState> CreatePipelineState(in CreatePipelineStateArgs args)
    {
        var handle = _pipelineStates.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager>($"Failed to allocate a slot for the {nameof(PipelineState)}");
            return Handle<PipelineState>.Invalid;
        }

        Debug.Assert(args.RootSignature.IsValid);
        var rootSignature = _rootSignatures.AsPtr(args.RootSignature.Value);
        Debug.Assert(rootSignature->Resource.IsValid);

        TitanList<DXGI_FORMAT> formats = stackalloc DXGI_FORMAT[10];
        foreach (var textureHandle in args.RenderTargets)
        {
            Debug.Assert(textureHandle.IsValid);
            var texture = _textures.AsPtr(textureHandle.Value);
            Debug.Assert(texture != null);
            formats.Add(texture->Format);
        }

        var pipelineState = _pipelineStates.AsPtr(handle);

        var psoStream = new D3D12PipelineSubobjectStream()
                .Blend(D3D12Helpers.GetBlendState(BlendStateType.AlphaBlend)) //TODO(Jens): Should be configurable, but keep it simple for now.
                .Topology(args.Topology)
                .Razterizer(D3D12_RASTERIZER_DESC.Default() with
                {
                    CullMode = D3D12_CULL_MODE.D3D12_CULL_MODE_BACK // TODO(Jens): Should be configurable
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

        return handle.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PipelineState* Access(in Handle<PipelineState> handle)
        => (PipelineState*)_pipelineStates.AsPtr(handle.Value);

    public void DestroyPipelineState(Handle<PipelineState> handle)
    {
        Debug.Assert(handle.IsValid);
        var pipeline = _pipelineStates.AsPtr(handle.Value);
        pipeline->Resource.Dispose();
        *pipeline = default;
        _pipelineStates.SafeFree(handle.Value);
    }
}


