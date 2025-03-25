using System;
using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.RenderingV3.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.RenderingV3;


public static class D3D12Extensions
{
    public static DXGI_FORMAT AsDxgiFormat(this TextureFormat format) =>
        format switch
        {
            TextureFormat.R8 => DXGI_FORMAT.DXGI_FORMAT_R8_UNORM,
            TextureFormat.R32 => DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT,
            TextureFormat.RGBA16F => DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT,
            TextureFormat.RGBA32F => DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
            TextureFormat.RGBA8 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
            TextureFormat.BGRA8 => DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
            TextureFormat.D32 => DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
            TextureFormat.BC7 => DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM,

            _ => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN
        };

    public static TextureFormat AsTextureFormat(this DXGI_FORMAT format) =>
        format switch
        {
            DXGI_FORMAT.DXGI_FORMAT_R8_UNORM => TextureFormat.R8,
            DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT => TextureFormat.R32,
            DXGI_FORMAT.DXGI_FORMAT_R16G16B16A16_FLOAT => TextureFormat.RGBA16F,
            DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT => TextureFormat.RGBA32F,
            DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM => TextureFormat.RGBA8,
            DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM => TextureFormat.BGRA8,
            DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT => TextureFormat.D32,
            DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM => TextureFormat.BC7,

            _ => TextureFormat.Unknown
        };


}

public record struct CreateTextureArgs1(uint Width, uint Height, TextureFormat Format, bool ShaderVisible = false, bool RenderTarget = false, bool UnorderedAccess = false);

public record struct CreateBufferArgs1(uint Stride, uint Count, BufferType Type, bool ShaderVisible = false, bool CpuVisible = false, bool RawAccess = false, bool UnorderedAccess = false)
{
    public uint Size => Stride * Count;
    public static unsafe CreateBufferArgs1 Structured<T>(uint count, bool shaderVisible = false, bool cpuVisible = false, bool rawAccess = false, bool unorderedAccess = false) where T : unmanaged
    {
        Debug.Assert(sizeof(T) % 16 == 0, "Structured buffer must be 16 byte aligned.");
        return new((uint)sizeof(T), count, BufferType.Structured, shaderVisible, rawAccess, unorderedAccess);
    }
}

[UnmanagedResource]
internal unsafe struct D3D12ResourceManager1
{
    private ResourcePool<Texture1> Textures;
    private ResourcePool<GPUBuffer1> Buffers;


    [System(SystemStage.PreInit)]
    public static void Init(ref D3D12ResourceManager1 resourceManager1, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();

        if (!memoryManager.TryCreateResourcePool(out resourceManager1.Textures, config.Resources.MaxTextures))
        {
            Logger.Error<D3D12ResourceManager1>($"Failed to create ResourcePool for {nameof(Texture1)}. Count = {config.Resources.MaxTextures}");
            return;
        }

        if (!memoryManager.TryCreateResourcePool(out resourceManager1.Buffers, config.Resources.MaxBuffers))
        {
            Logger.Error<D3D12ResourceManager1>($"Failed to create ResourcePool for {nameof(GPUBuffer1)}. Count = {config.Resources.MaxBuffers}");
            return;
        }

    }

    [System(SystemStage.EndOfLife)]
    public static void Shutdown(ref D3D12ResourceManager1 resourceManager1, IMemoryManager memoryManager)
    {
        memoryManager.FreeResourcePool(ref resourceManager1.Buffers);
        memoryManager.FreeResourcePool(ref resourceManager1.Textures);
    }

    public Handle<GPUBuffer1> CreateBuffer(D3D12Context* context, in CreateBufferArgs1 args, string? name = null)
    {
        Debug.Assert(args.Type != BufferType.Index || args.);

        D3D12_RESOURCE_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            MipLevels = 0,
            Flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE,
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER,
            Width = args.Size,
            Alignment = 0,
            Height = 1,
            DepthOrArraySize = 1,
            SampleDesc = { Count = 1, Quality = 0 },
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_ROW_MAJOR
        };

        var flags = args.ShaderVisible
            ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE
            : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        var heapType = args.CpuVisible
            ? D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD
            : D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT;

        //TODO(Jens): Figure out what state we should create buffers in
        var state = args.Type is BufferType.IndirectArguments
            ? D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_INDIRECT_ARGUMENT
            : D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;


        var resource = D3D12Device.CreateResource(context->Device, desc, state, heapType, name: name);
        if (resource == null)
        {
            Logger.Error<D3D12ResourceManager1>($"Failed to create the Buffer. Name = {name}");
            return Handle<GPUBuffer1>.Invalid;
        }

        var handle = Buffers.SafeAlloc();
        if (handle.IsInvalid)
        {
            Logger.Error<D3D12ResourceManager1>($"Failed to allocate a handle for the Buffer. Name = {name}");
            resource->Release();
            return Handle<GPUBuffer1>.Invalid;
        }

        var buffer = Buffers.AsPtr(handle);
        buffer->Resource = resource;
        buffer->Size = args.Size;
        buffer->Type = args.Type;

        if (args.ShaderVisible)
        {
            buffer->SRV = context->AllocDescriptor(DescriptorHeapTypes.ShaderResourceView);

            // if its a raw buffer we just split the count with 4.
            var count = args.RawAccess ? args.Count / 4 : args.Count;
            var stride = args.RawAccess ? 0 : args.Stride;
            var format = args.RawAccess ? DXGI_FORMAT.DXGI_FORMAT_R32_TYPELESS : DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
            var srvFlags = args.RawAccess ? D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_RAW : 0;

            D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = new()
            {
                Format = format,
                Shader4ComponentMapping = D3D12Constants.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,
                ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER,
                Buffer = new()
                {
                    Flags = srvFlags,
                    NumElements = count,
                    StructureByteStride = stride
                }
            };

            D3D12Device.CreateShaderResourceView(context->Device, resource, srvDesc, context->GetCpuDescriptorHandle(buffer->SRV));

        }

        return handle;
    }



    public Handle<Texture1> CreateTexture(D3D12Context* context, in CreateTextureArgs1 args, string? name = null)
    {
        var flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;
        var state = D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;
        if (args.RenderTarget)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET;
        }

        if (args.Format == TextureFormat.D32)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
        }

        if (args.UnorderedAccess)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
        }

        if (!args.ShaderVisible)
        {
            flags |= D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_DENY_SHADER_RESOURCE;
        }

        var desc = new D3D12_RESOURCE_DESC
        {
            Width = args.Width,
            Height = args.Height,
            Format = args.Format.AsDxgiFormat(),
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Flags = flags,
            DepthOrArraySize = 1,
            Alignment = 0,
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D,
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_UNKNOWN,
            MipLevels = 0
        };

        var resource = D3D12Device.CreateResource(context->Device, desc, state, D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT, name: name);
        if (resource == null)
        {
            Logger.Error<D3D12ResourceManager1>($"Failed to create the Texture. Name = {name}");
            return Handle<Texture1>.Invalid;
        }

        var handle = Textures.SafeAlloc();
        if (handle.IsInvalid)
        {
            //TODO(Jens): Maybe this should be an Assert instead.
            Logger.Error<D3D12ResourceManager1>($"Failed to allocate a handle for the Texture. Name = {name}");
            resource->Release();
            return Handle<Texture1>.Invalid;
        }

        var texture = Textures.AsPtr(handle);
        texture->Format = args.Format;
        texture->Height = args.Height;
        texture->Width = args.Width;
        texture->Resource = resource;

        if (args.ShaderVisible)
        {
            texture->SRV = context->AllocDescriptor(DescriptorHeapTypes.ShaderResourceView);
            Debug.Assert(texture->SRV.IsValid);
            D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = new()
            {
                Format = args.Format.AsDxgiFormat(),
                Shader4ComponentMapping = D3D12Constants.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,
                ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_TEXTURE2D,
                Texture2D = new() { MipLevels = 1 }
            };
            D3D12Device.CreateShaderResourceView(context->Device, resource, srvDesc, context->GetCpuDescriptorHandle(texture->SRV));
        }

        if (args.RenderTarget)
        {
            texture->RTV = context->AllocDescriptor(DescriptorHeapTypes.RenderTargetView);
            Debug.Assert(texture->RTV.IsValid);
            D3D12Device.CreateRenderTargetView(context->Device, resource, context->GetCpuDescriptorHandle(texture->RTV));
        }

        if (args.UnorderedAccess)
        {
            texture->UAV = context->AllocDescriptor(DescriptorHeapTypes.UnorderedAccessView);
            Debug.Assert(texture->UAV.IsValid);
            D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = new()
            {
                Format = args.Format.AsDxgiFormat(),
                ViewDimension = D3D12_UAV_DIMENSION.D3D12_UAV_DIMENSION_TEXTURE2D,
                Texture2D = new()
                {
                    PlaneSlice = 0,
                    MipSlice = 0
                }
            };
            D3D12Device.CreateUnorderedAccessView(context->Device, resource, uavDesc, context->GetCpuDescriptorHandle(texture->UAV));
        }

        return handle;
    }

    public Handle<PipelineState> CreatePipelineState(D3D12Context* context, in CreatePipelineStateArgs args)
    {
        //D3D12Device.CreatePipelineState(context->Device);

        return default;
    }

    public ID3D12PipelineState* GetPipelineState(Handle<PipelineState> handle)
    {
        if (handle.IsInvalid)
        {
            return null;

        }

        throw new NotImplementedException();
    }


    public void UploadBuffer(D3D12Context* context, Handle<GPUBuffer1> handle, TitanBuffer data, ulong destinationOffset = 0, ulong sourceOffset = 0)
    {
        Debug.Assert(context != null);
        Debug.Assert(handle.IsValid);
        Debug.Assert(data.IsValid);

        var buffer = Buffers.AsPtr(handle);
        Debug.Assert(buffer->Size >= destinationOffset + data.Size);

        var destination = (ID3D12Resource*)buffer->Resource;

        D3D12_HEAP_PROPERTIES heapProperties;
        destination->GetHeapProperties(&heapProperties, null);

        // CPU visible resource, we can just map and copy.
        if (heapProperties.Type is D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_READBACK or D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD)
        {
            void* ptr;
            //TODO(Jens): Verify that this works as expected. otherwise use the default range, and copy with an offset.
            D3D12_RANGE range = new()
            {
                Begin = (nuint)destinationOffset,
                End = (nuint)(destinationOffset + data.Size)
            };
            destination->Map(0, &range, &ptr);
            MemoryUtils.Copy(ptr, data.AsReadOnlySpan());
            destination->Unmap(0, null);
            return;
        }

        using ComPtr<ID3D12Resource> tempUploadBuffer = CreateUploadBuffer(context, data.Size);

        {
            D3D12_RANGE range = default;
            void* ptr;
            tempUploadBuffer.Get()->Map(0, &range, &ptr);
            Debug.Assert(ptr != null);
            MemoryUtils.Copy(ptr, data.AsReadOnlySpan());
        }

        SpinWait wait = new();
        int index;
        while (!context->CopyCommandLists.TryGetAvailableCommandList(out index))
        {
            wait.SpinOnce();
        }

        var allocator = context->CopyCommandLists.Allocator[index].Get();
        var commandList = context->CopyCommandLists.CommandList[index].Get();
        commandList->Reset(allocator, null);
        commandList->CopyBufferRegion(destination, destinationOffset, tempUploadBuffer, 0, data.Size);
        commandList->Close();
        context->CopyCommandLists.ExecuteCommandList(context->GetCommandQueue(CommandQueueTypes.Copy), index);
    }

    private static ID3D12Resource* CreateUploadBuffer(D3D12Context* context, uint size)
    {
        D3D12_RESOURCE_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            Width = size,
            Height = 1,
            MipLevels = 1,
            DepthOrArraySize = 1,
            Flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE,
            Alignment = 0,
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_ROW_MAJOR,
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            }
        };
        return D3D12Device.CreateResource(context->Device, desc, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD, name: $"Temp Upload Buffer. Size = {size}");
    }

    public bool UploadTexture(D3D12Context* context, Handle<Texture1> handle, TitanBuffer data)
    {
        Debug.Assert(context != null);
        Debug.Assert(handle.IsValid);
        Debug.Assert(data.IsValid);

        var texture = Textures.AsPtr(handle);
        Debug.Assert(texture->IsValid());

        var destination = (ID3D12Resource*)texture->Resource;

        // Upload to a temporary buffer that is CPU visible/Mappable
        using ComPtr<ID3D12Resource> tempBuffer = CreateUploadBuffer(context, data.Size);

        {
            D3D12_RANGE range = default;
            void* ptr;
            tempBuffer.Get()->Map(0, &range, &ptr);
            Debug.Assert(ptr != null);
            MemoryUtils.Copy(ptr, data.AsReadOnlySpan());
        }


        // Create the footprint for the Texture

        D3D12_RESOURCE_DESC resourceDesc;
        destination->GetDesc(&resourceDesc);

        D3D12_PLACED_SUBRESOURCE_FOOTPRINT footprint;
        context->Device.Get()->GetCopyableFootprints(&resourceDesc, 0, 1, 0, &footprint, null, null, null);

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


        // Get a command list and call CopyTextureRegion
        SpinWait wait = new();
        int index;
        while (!context->CopyCommandLists.TryGetAvailableCommandList(out index))
        {
            wait.SpinOnce();
        }

        var allocator = context->CopyCommandLists.Allocator[index].Get();
        var commandList = context->CopyCommandLists.CommandList[index].Get();
        commandList->Reset(allocator, null);
        commandList->CopyTextureRegion(&copyDst, 0, 0, 0, &copySrc, null);
        commandList->Close();
        context->CopyCommandLists.ExecuteCommandList(context->GetCommandQueue(CommandQueueTypes.Copy), index);

        return false;
    }
}

