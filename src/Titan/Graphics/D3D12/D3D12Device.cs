using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12.Adapters;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Graphics.D3D12;

[UnmanagedResource]
internal unsafe partial struct D3D12Device
{
    public ComPtr<ID3D12Device4> Device;

    public static implicit operator ID3D12Device4*(in D3D12Device device) => device.Device.Get();

    [System(SystemStage.PreInit)]
    public static void PreInit(ref D3D12Device device, in D3D12Adapter d3d12Adapter, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        ref readonly var adapter = ref d3d12Adapter.PrimaryAdapter;

        Logger.Trace<D3D12Device>($"Creating a {nameof(ID3D12Device4)} with FeatureLevel {config.FeatureLevel}.");

        using var timer = new MeasureTime<D3D12Device>("Created device in {0} ms.");
        var hr = D3D12CreateDevice((IUnknown*)adapter.Adapter.Get(), config.FeatureLevel, ID3D12Device4.Guid, (void**)device.Device.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12Device4)} with feature level {config.FeatureLevel}. HRESULT = {hr}");
        }
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(ref D3D12Device device)
    {
        Logger.Trace<D3D12Device>($"Destroying the {nameof(ID3D12Device4)}.");
        device.Device.Dispose();
    }


    public readonly ID3D12CommandQueue* CreateCommandQueue(D3D12_COMMAND_LIST_TYPE type)
    {
        var device = Device.Get();
        D3D12_COMMAND_QUEUE_DESC desc = new()
        {
            Flags = D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE,
            NodeMask = 0,
            Priority = 0,
            Type = type
        };
        ID3D12CommandQueue* queue;
        var hr = device->CreateCommandQueue(&desc, ID3D12CommandQueue.Guid, (void**)&queue);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12CommandQueue)}. HRESULT = {hr}");
            return null;
        }

        return queue;
    }

    public readonly ID3D12GraphicsCommandList4* CreateGraphicsCommandList(D3D12_COMMAND_LIST_TYPE type, string? name = null)
    {
        ID3D12GraphicsCommandList4* commandList;
        var hr = Device.Get()->CreateCommandList1(0, type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, ID3D12GraphicsCommandList4.Guid, (void**)&commandList);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12GraphicsCommandList4)}. HRESULT = {hr}");
            return null;
        }

        if (name != null)
        {
            D3D12Helpers.SetName(commandList, name);
        }

        return commandList;
    }

    public readonly ID3D12CommandAllocator* CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE type, string? name = null)
    {
        ID3D12CommandAllocator* allocator;
        var hr = Device.Get()->CreateCommandAllocator(type, ID3D12CommandAllocator.Guid, (void**)&allocator);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12CommandAllocator)}. HRESULT = {hr}");
            return null;
        }

        if (name != null)
        {
            D3D12Helpers.SetName(allocator, name);
        }

        return allocator;
    }

    public readonly ID3D12Fence* CreateFence()
    {
        ID3D12Fence* fence;
        var hr = Device.Get()->CreateFence(0, D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE, ID3D12Fence.Guid, (void**)&fence);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Fence)}. HRESULT = {hr}");
            return null;
        }
        return fence;
    }

    public readonly ID3D12DescriptorHeap* CreateDescriptorHeap(D3D12_DESCRIPTOR_HEAP_TYPE type, uint count, bool shaderVisible)
    {
        ID3D12DescriptorHeap* heap;
        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Flags = shaderVisible ? D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE : 0,
            NodeMask = 0,
            NumDescriptors = count,
            Type = type
        };
        var hr = Device.Get()->CreateDescriptorHeap(&desc, ID3D12DescriptorHeap.Guid, (void**)&heap);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12DescriptorHeap)}. HRESULT = {hr}");
            return null;
        }

        return heap;
    }

    public readonly void CreateConstantBufferView(uint sizeInBytes, D3D12_GPU_VIRTUAL_ADDRESS address, D3D12_CPU_DESCRIPTOR_HANDLE handle)
    {

        D3D12_CONSTANT_BUFFER_VIEW_DESC desc = new()
        {
            BufferLocation = address,
            SizeInBytes = sizeInBytes
        };
        Device.Get()->CreateConstantBufferView(&desc, handle);
    }

    public readonly void CreateShaderResourceView(ID3D12Resource* resource, DXGI_FORMAT format, D3D12_CPU_DESCRIPTOR_HANDLE handle)
    {
        Logger.Warning<D3D12Device>($"The method {nameof(CreateShaderResourceView)} only supports texture2d without mips, not really a good solution.");
        D3D12_SHADER_RESOURCE_VIEW_DESC desc = new()
        {
            Format = format,
            Shader4ComponentMapping = D3D12Constants.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,
            ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_TEXTURE2D,
            Texture2D = new D3D12_TEX2D_SRV
            {
                MipLevels = 1,
                MostDetailedMip = 0,
                PlaneSlice = 0,
                ResourceMinLODClamp = 0
            }
        };
        Device.Get()->CreateShaderResourceView(resource, &desc, handle);
    }

    public readonly void CreateShaderResourceView1(ID3D12Resource* resource, D3D12_CPU_DESCRIPTOR_HANDLE handle, uint numberOfVertices, uint stride)
    {
        Logger.Warning<D3D12Device>($"The method {nameof(CreateShaderResourceView)} only supports texture2d without mips, not really a good solution.");
        D3D12_SHADER_RESOURCE_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            Shader4ComponentMapping = D3D12Constants.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING,
            ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER,
            Buffer = new()
            {
                FirstElement = 0,
                Flags = D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_NONE,
                NumElements = numberOfVertices,
                StructureByteStride = stride
            }
        };
        Device.Get()->CreateShaderResourceView(resource, &desc, handle);
    }
    public readonly ID3D12PipelineState* CreatePipelineStateObject(D3D12_PIPELINE_STATE_STREAM_DESC desc)
    {
        ID3D12PipelineState* pipelineState;
        var hr = Device.Get()->CreatePipelineState(&desc, ID3D12PipelineState.Guid, (void**)&pipelineState);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12PipelineState)}. HRESULT = {hr}");
            return null;
        }
        return pipelineState;
    }

    public readonly ID3D12Resource* CreateBuffer(uint size, bool isCpuVisible = false, D3D12_RESOURCE_STATES state = D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE)
    {
        //NOTE(Jens): These should be handled by the caller and not by the device. When we do that change we'll manage all memory by ourselves and not use CommittedResource
        var heap = D3D12Helpers.GetHeap(isCpuVisible ? D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD : D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);
        D3D12_RESOURCE_DESC resourceDesc = new()
        {
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_BUFFER,
            Width = size,
            Height = 1,
            MipLevels = 1,
            DepthOrArraySize = 1,
            Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Alignment = 0,
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_ROW_MAJOR,
            Flags = isCpuVisible ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE : flags
        };

        var resourceState = isCpuVisible ? D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_GENERIC_READ : state;
        ID3D12Resource* resource;
        var hr = Device.Get()->CreateCommittedResource1(heap, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE, &resourceDesc, resourceState, null, null, ID3D12Resource.Guid, (void**)&resource);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Resource)} with HRESULT {hr}");
            return null;
        }
        return resource;
    }

    public readonly ID3D12Resource* CreateTexture(int width, int height, DXGI_FORMAT format, D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE, D3D12_CLEAR_VALUE* clearValue = null)
    {
        Debug.Assert(width >= 0 && height >= 0);
        return CreateTexture((uint)width, (uint)height, format, flags, clearValue);
    }

    public readonly ID3D12Resource* CreateTexture(uint width, uint height, DXGI_FORMAT format, D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE, D3D12_CLEAR_VALUE* clearValue = null)
    {
        //NOTE(Jens): Add support for Mip levels etc.
        D3D12_RESOURCE_DESC resourceDesc = new()
        {
            Width = width,
            Height = height,
            Format = format,
            Flags = flags,
            DepthOrArraySize = 1, // change this when we support other types of textures.
            Alignment = 0,
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_UNKNOWN,
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D,
            MipLevels = 1,
            SampleDesc = new DXGI_SAMPLE_DESC
            {
                Count = 1,
                Quality = 0
            }
        };

        ID3D12Resource* resource;
        var heap = D3D12Helpers.GetHeap(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);
        var hr = Device.Get()->CreateCommittedResource1(
            heap,
            D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
            &resourceDesc,
            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON,
            clearValue,
            (ID3D12ProtectedResourceSession*)null,
            ID3D12Resource.Guid,
            (void**)&resource
        );
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a Texture ({nameof(ID3D12Resource)}). HRESULT = {hr}");
            return null;
        }

        return resource;
    }
    public readonly ID3D12RootSignature* CreateRootSignature(D3D12_ROOT_SIGNATURE_FLAGS flags, ReadOnlySpan<D3D12_ROOT_PARAMETER1> parameters, ReadOnlySpan<D3D12_STATIC_SAMPLER_DESC> staticSamplers)
    {
        HRESULT hr;
        using ComPtr<ID3DBlob> blob = default;
        using ComPtr<ID3DBlob> error = default;

        ID3D12RootSignature* rootSignature;

        fixed (D3D12_ROOT_PARAMETER1* pParameters = parameters)
        fixed (D3D12_STATIC_SAMPLER_DESC* pSamplers = staticSamplers)
        {
            var desc = new D3D12_ROOT_SIGNATURE_DESC1
            {
                Flags = flags,
                pParameters = pParameters,
                pStaticSamplers = pSamplers,
                NumParameters = (uint)parameters.Length,
                NumStaticSamplers = (uint)staticSamplers.Length
            };

            var versioned = new D3D12_VERSIONED_ROOT_SIGNATURE_DESC
            {
                Desc_1_1 = desc,
                Version = D3D_ROOT_SIGNATURE_VERSION.D3D_ROOT_SIGNATURE_VERSION_1_1
            };

            hr = D3D12SerializeVersionedRootSignature(&versioned, blob.GetAddressOf(), error.GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<D3D12Device>($"Failed to serialize the root signature with HRESULT {hr}");

#if DEBUG
                var span = new ReadOnlySpan<byte>(error.Get()->GetBufferPointer(), (int)error.Get()->GetBufferSize());
                Logger.Error<D3D12Device>($"Error: {Encoding.UTF8.GetString(span)}");
#endif
                return null;
            }
        }
        hr = Device.Get()->CreateRootSignature(0, blob.Get()->GetBufferPointer(), blob.Get()->GetBufferSize(), ID3D12RootSignature.Guid, (void**)&rootSignature);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12RootSignature)}. HRESULT = {hr}");
            return null;
        }
        return rootSignature;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetDescriptorHandleIncrementSize(DescriptorHeapType type)
    {
        var d3d12Type = type switch
        {
            DescriptorHeapType.ShaderResourceView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
            DescriptorHeapType.RenderTargetView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV,
            DescriptorHeapType.DepthStencilView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV,
            DescriptorHeapType.UnorderedAccessView => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
            DescriptorHeapType.Count => D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_NUM_TYPES,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
        return Device.Get()->GetDescriptorHandleIncrementSize(d3d12Type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE type)
        => Device.Get()->GetDescriptorHandleIncrementSize(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CreateRenderTargetView(ID3D12Resource* resource, in D3D12_RENDER_TARGET_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        fixed (D3D12_RENDER_TARGET_VIEW_DESC* pDesc = &desc)
        {
            CreateRenderTargetView(resource, pDesc, cpuHandle);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CreateRenderTargetView(ID3D12Resource* resource, D3D12_RENDER_TARGET_VIEW_DESC* desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
        => Device.Get()->CreateRenderTargetView(resource, desc, cpuHandle);

    public readonly ID3D12Resource* CreateDepthBuffer(uint width, uint height, float depthClearValue = 1.0f, byte stencilClearValue = 0)
    {
        D3D12_CLEAR_VALUE clearValue = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
            DepthStencil = new()
            {
                Depth = depthClearValue,
                Stencil = stencilClearValue
            }
        };

        D3D12_RESOURCE_DESC desc = new()
        {
            Width = width,
            Height = height,
            Format = DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
            Dimension = D3D12_RESOURCE_DIMENSION.D3D12_RESOURCE_DIMENSION_TEXTURE2D,
            DepthOrArraySize = 1,
            MipLevels = 1,
            SampleDesc =
            {
                Count = 1,
                Quality = 0
            },
            Layout = D3D12_TEXTURE_LAYOUT.D3D12_TEXTURE_LAYOUT_UNKNOWN,
            Flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL
        };

        var heap = D3D12Helpers.GetHeap(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);
        ID3D12Resource* resource;
        var hr = Device.Get()->CreateCommittedResource(heap, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE, &desc, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_WRITE, &clearValue, ID3D12Resource.Guid, (void**)&resource);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the DepthBuffer. HRESULT = {hr}");
            return null;
        }
        return resource;
    }

    public readonly void CreateDepthStencilView(ID3D12Resource* resource, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        D3D12_DEPTH_STENCIL_VIEW_DESC desc = new()
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
            ViewDimension = D3D12_DSV_DIMENSION.D3D12_DSV_DIMENSION_TEXTURE2D,
            Flags = D3D12_DSV_FLAGS.D3D12_DSV_FLAG_NONE
        };
        Device.Get()->CreateDepthStencilView(resource, &desc, cpuHandle);
    }
}
