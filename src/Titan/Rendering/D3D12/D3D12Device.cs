using System.Text;
using Titan.Application.Services;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering.D3D12.Adapters;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12;
internal sealed unsafe class D3D12Device : IService
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

    private ComPtr<ID3D12Device4> _device;

    internal ID3D12Device4* Device => _device;
    public bool Init(D3D12Adapter adapter, D3D_FEATURE_LEVEL featureLevel)
    {
        var hr = D3D12CreateDevice(adapter.PrimaryAdapter, featureLevel, _device.UUID, (void**)_device.GetAddressOf());

        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Device4)}. HRESULT = {hr}");
            return false;
        }
        return true;
    }


    public void Shutdown()
    {
        _device.Dispose();
    }

    public ID3D12CommandQueue* CreateCommandQueue(D3D12_COMMAND_LIST_TYPE type)
    {
        D3D12_COMMAND_QUEUE_DESC desc = new()
        {
            Flags = D3D12_COMMAND_QUEUE_FLAGS.D3D12_COMMAND_QUEUE_FLAG_NONE,
            NodeMask = 0,
            Priority = 0,
            Type = type
        };
        ID3D12CommandQueue* commandQueue;
        var hr = _device.Get()->CreateCommandQueue(&desc, ID3D12CommandQueue.Guid, (void**)&commandQueue);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12CommandQueue)}. HRESULT = {hr}");
            return null;
        }
        return commandQueue;
    }

    public ID3D12GraphicsCommandList4* CreateCommandList(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12GraphicsCommandList4* commandList;
        var hr = _device.Get()->CreateCommandList1(0, type, D3D12_COMMAND_LIST_FLAGS.D3D12_COMMAND_LIST_FLAG_NONE, ID3D12GraphicsCommandList4.Guid, (void**)&commandList);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12GraphicsCommandList4)}. HRESULT = {hr}");
            return null;
        }

        return commandList;
    }

    public ID3D12CommandAllocator* CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE type)
    {
        ID3D12CommandAllocator* allocator;
        var hr = _device.Get()->CreateCommandAllocator(type, ID3D12CommandAllocator.Guid, (void**)&allocator);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12CommandAllocator)}. HRESULT = {hr}");
            return null;
        }

        return allocator;
    }

    public ID3D12DescriptorHeap* CreateDescriptorHeap(D3D12_DESCRIPTOR_HEAP_TYPE type, uint numberOfDescriptors, bool shaderVisible)
    {
        if (type is D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV or D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV)
        {
            shaderVisible = false;
        }

        var flags = shaderVisible
            ? D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE
            : D3D12_DESCRIPTOR_HEAP_FLAGS.D3D12_DESCRIPTOR_HEAP_FLAG_NONE;

        D3D12_DESCRIPTOR_HEAP_DESC desc = new()
        {
            Flags = flags,
            NodeMask = 0,
            NumDescriptors = numberOfDescriptors,
            Type = type
        };
        ID3D12DescriptorHeap* descriptorHeap;
        var hr = _device.Get()->CreateDescriptorHeap(&desc, ID3D12DescriptorHeap.Guid, (void**)&descriptorHeap);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create a {nameof(ID3D12DescriptorHeap)}. HRESULT = {hr}");
            return null;
        }

        return descriptorHeap;
    }

    public uint GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE type)
        => _device.Get()->GetDescriptorHandleIncrementSize(type);


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
        var hr = Device->CreateCommittedResource1(&heap, D3D12_HEAP_FLAGS.D3D12_HEAP_FLAG_NONE, &resourceDesc, resourceState, null, null, ID3D12Resource.Guid, (void**)&resource);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Resource)} with HRESULT {hr}");
            return null;
        }
        return resource;
    }

    public void CreateShaderResourceView(ID3D12Resource* resource, in D3D12_SHADER_RESOURCE_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        fixed (D3D12_SHADER_RESOURCE_VIEW_DESC* pDesc = &desc)
        {
            _device.Get()->CreateShaderResourceView(resource, pDesc, cpuHandle);
        }
    }

    public void CreateRenderTargetView(ID3D12Resource* resource, in D3D12_RENDER_TARGET_VIEW_DESC desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
    {
        fixed (D3D12_RENDER_TARGET_VIEW_DESC* pDesc = &desc)
        {
            CreateRenderTargetView(resource, pDesc, cpuHandle);
        }
    }
    public void CreateRenderTargetView(ID3D12Resource* resource, D3D12_RENDER_TARGET_VIEW_DESC* desc, D3D12_CPU_DESCRIPTOR_HANDLE cpuHandle)
        => _device.Get()->CreateRenderTargetView(resource, desc, cpuHandle);

    public ID3D12Fence* CreateFence(D3D12_FENCE_FLAGS flags = D3D12_FENCE_FLAGS.D3D12_FENCE_FLAG_NONE, uint initialValue = 0)
    {
        ID3D12Fence* fence;
        var hr = Device->CreateFence(initialValue, flags, ID3D12Fence.Guid, (void**)&fence);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Fence)}. HRESULT = {hr}");
            return null;
        }
        return fence;
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
        var hr = Device->CreateCommittedResource1(
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

    public ID3D12PipelineState* CreatePipelineStateObject(D3D12_PIPELINE_STATE_STREAM_DESC desc)
    {
        ID3D12PipelineState* pipelineState;
        var hr = Device->CreatePipelineState(&desc, ID3D12PipelineState.Guid, (void**)&pipelineState);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to crate the {nameof(ID3D12PipelineState)}. HRESULT = {hr}");
            return null;
        }
        return pipelineState;
    }

    public ID3D12RootSignature* CreateRootSignature(D3D12_ROOT_SIGNATURE_FLAGS flags, ReadOnlySpan<D3D12_ROOT_PARAMETER1> parameters, ReadOnlySpan<D3D12_STATIC_SAMPLER_DESC> staticSamplers)
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

        hr = Device->CreateRootSignature(0, blob.Get()->GetBufferPointer(), blob.Get()->GetBufferSize(), ID3D12RootSignature.Guid, (void**)&rootSignature);
        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12RootSignature)}. HRESULT = {hr}");
            return null;
        }
        return rootSignature;
    }
}
