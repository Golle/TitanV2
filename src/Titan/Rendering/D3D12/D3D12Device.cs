using System.Runtime.CompilerServices;
using System.Text;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering.D3D12.Adapters;
using Titan.Rendering.D3D12.Memory;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12;

[UnmanagedResource]
internal unsafe partial struct D3D12Device
{
    //NOTE(Jens): These heaps should be managed by the caller, and not by the device
    private static readonly D3D12_HEAP_PROPERTIES DefaultHeap = new()
    {
        Type = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT,
        CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
        MemoryPoolPreference = D3D12_MEMORY_POOL.D3D12_MEMORY_POOL_UNKNOWN,
        CreationNodeMask = 0,
        VisibleNodeMask = 0
    };

    //NOTE(Jens): These heaps should be managed by the caller, and not by the device
    private static readonly D3D12_HEAP_PROPERTIES UploadHeap = new()
    {
        Type = D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_UPLOAD,
        CPUPageProperty = D3D12_CPU_PAGE_PROPERTY.D3D12_CPU_PAGE_PROPERTY_UNKNOWN,
        MemoryPoolPreference = D3D12_MEMORY_POOL.D3D12_MEMORY_POOL_UNKNOWN,
        CreationNodeMask = 0,
        VisibleNodeMask = 0
    };

    public ComPtr<ID3D12Device4> Device;

    public static implicit operator ID3D12Device4*(in D3D12Device device) => device.Device.Get();

    [System(SystemStage.Init)]
    public static void Init(D3D12Device* device, in D3D12Adapter d3d12Adapter, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        ref readonly var adapter = ref d3d12Adapter.PrimaryAdapter;

        Logger.Trace<D3D12Device>($"Creating a {nameof(ID3D12Device4)} with FeatureLevel {config.FeatureLevel}.");

        using var _ = new MeasureTime<D3D12Device>("Created device in {0} ms.");
        var hr = D3D12CreateDevice((IUnknown*)adapter.Adapter.Get(), config.FeatureLevel, ID3D12Device4.Guid, (void**)device->Device.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12Device4)} with feature level {config.FeatureLevel}. HRESULT = {hr}");
        }
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(D3D12Device* device)
    {
        Logger.Trace<D3D12Device>($"Destroying the {nameof(ID3D12Device4)}.");
        device->Device.Dispose();
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

    public readonly ID3D12GraphicsCommandList4* CreateGraphicsCommandList(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12GraphicsCommandList4* commandList;
        var hr = Device.Get()->CreateCommandList1(0, type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, ID3D12GraphicsCommandList4.Guid, (void**)&commandList);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12GraphicsCommandList4)}. HRESULT = {hr}");
            return null;
        }

        return commandList;
    }

    public readonly ID3D12CommandAllocator* CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12CommandAllocator* allocator;
        var hr = Device.Get()->CreateCommandAllocator(type, ID3D12CommandAllocator.Guid, (void**)&allocator);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12CommandAllocator)}. HRESULT = {hr}");
            return null;
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

    public ID3D12Resource* CreateBuffer(uint size, bool isCpuVisible = false, D3D12_RESOURCE_STATES state = D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_FLAGS flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE)
    {
        //NOTE(Jens): These should be handled by the caller and not by the device. When we do that change we'll manage all memory by ourselves and not use CommittedResource
        var heap = isCpuVisible ? UploadHeap : DefaultHeap;
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
        var hr = Device.Get()->CreateCommittedResource1(&heap, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE, &resourceDesc, resourceState, null, null, ID3D12Resource.Guid, (void**)&resource);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Resource)} with HRESULT {hr}");
            return null;
        }
        return resource;
    }

    public ID3D12Resource* CreateTexture(uint width, uint height, DXGI_FORMAT format)
    {
        //NOTE(Jens): Add support for Mip levels etc.
        D3D12_RESOURCE_DESC resourceDesc = new()
        {
            Width = width,
            Height = height,
            Format = format,
            Flags = D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE,
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
        var heapProperties = DefaultHeap;
        var hr = Device.Get()->CreateCommittedResource1(
            &heapProperties,
            D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE,
            &resourceDesc,
            D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON,
            (D3D12_CLEAR_VALUE*)null,
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
}
