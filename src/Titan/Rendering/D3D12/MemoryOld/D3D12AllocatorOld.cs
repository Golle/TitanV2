//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using Titan.Core.Logging;
//using Titan.Core.Memory;
//using Titan.Platform.Win32;
//using Titan.Platform.Win32.D3D12;
//using Titan.Platform.Win32.DXGI;
//using Titan.Rendering.D3D12.Utils;
//using Titan.Rendering.D3D12New.Memory;

//namespace Titan.Rendering.D3D12.Memory;

//internal unsafe struct TempBufferDescriptor
//{
//    public ComPtr<ID3D12Resource> Resource;
//    public void* CPUAddress;
//    public ulong GPUAddress;
//    public volatile uint Offset;
//    public DescriptorHandle SRV;
//}

//internal sealed unsafe class D3D12AllocatorOld : IService
//{
//    private const uint ConstantBufferAlignment = 256;
//    private const uint BufferCount = GlobalConfiguration.MaxRenderFrames;
//    private DescriptorHeaps _heaps;
//    private TempBuffersDescriptors _tempBuffers;
//    private D3D12Device? _device;
//    private IMemoryManager? _memoryManager;
//    private int _frameIndex;
//    private uint _tempBufferSize;
//    public bool Init(IMemoryManager memoryManager, D3D12Device device, in GPUMemoryConfig config)
//    {
//        for (var i = 0; i < (int)DescriptorHeapType.Count; ++i)
//        {
//            var type = (DescriptorHeapType)i;
//            var tempCount = type == DescriptorHeapType.ShaderResourceView ? config.TempShaderResourceViewCount : 0;
//            var shaderVisible = type == DescriptorHeapType.ShaderResourceView;
//            var count = config.GetDescriptorCount(type);
//            if (!_heaps[i].Init(memoryManager, device, type, count, tempCount, shaderVisible))
//            {
//                Logger.Error<D3D12AllocatorOld>($"Failed to initialize the {nameof(DescriptorHeapOld)}. Type = {type} Count = {count} ShaderVisibile = {shaderVisible} TempCount = {tempCount}");
//                return false;
//            }
//        }

//        // Init the temp buffers
//        var tempBufferSize = MemoryUtils.AlignToUpper(config.TempBufferSize, ConstantBufferAlignment);
//        for (var i = 0; i < BufferCount; ++i)
//        {
//            var resource = device.CreateBuffer(tempBufferSize, true, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
//            if (resource == null)
//            {
//                Logger.Error<D3D12AllocatorOld>($"Failed to create the temporary buffer. Size = {tempBufferSize} bytes Index = {i}");
//                return false;
//            }
//            D3D12Helpers.SetName(resource, $"TempConstantBuffer_{i}");
//            var desc = new TempBufferDescriptor
//            {
//                Resource = resource,
//                GPUAddress = resource->GetGPUVirtualAddress(),
//                SRV = Allocate(DescriptorHeapType.ShaderResourceView)
//            };
//            var hr = resource->Map(0, null, &desc.CPUAddress);
//            if (Win32Common.FAILED(hr))
//            {
//                Logger.Error<D3D12AllocatorOld>($"Failed to map the temporary buffer. Index = {i} HRESULT = {hr}");
//                return false;
//            }
//            _tempBuffers[i] = desc;
//        }

//        _memoryManager = memoryManager;
//        _device = device;
//        _tempBufferSize = tempBufferSize;


//        return true;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public DescriptorHandle Allocate(DescriptorHeapType type)
//    {
//        Debug.Assert(type != DescriptorHeapType.Count);
//        return _heaps[type].Allocate();
//    }

//    public void Free(in DescriptorHandle handle)
//    {
//        Debug.Assert(handle.Type != DescriptorHeapType.Count);
//        _heaps[handle.Type].Free(handle);
//    }


//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public StructuredBuffer<T> AllocateTempStructuredBuffer<T>(int count, bool createDescriptor) where T : unmanaged
//    {
//        Debug.Assert(count >= 0);
//        return AllocateTempStructuredBuffer<T>((uint)count, createDescriptor);
//    }

//    public StructuredBuffer<T> AllocateTempStructuredBuffer<T>(uint count, bool createDescriptor) where T : unmanaged
//    {
//        Debug.Assert(_device != null);
//        //Debug.Assert(int.IsPow2(sizeof(T)), $"A structured buffer must have a size that is a power of 2. Type {typeof(T).Name} has a size of {sizeof(T)}");
//        var stride = (uint)sizeof(T);
//        var size = stride * count;
//        var mappedMemory = GetTempMemory(size, stride);
//        var tempBuffer = new StructuredBuffer<T>
//        {
//            GPUAddress = mappedMemory.GPUAddress,
//            CPUAddress = mappedMemory.CPUAddress,
//            Count = count
//        };

//        Debug.Assert(mappedMemory.ResourceOffset % stride == 0);

//        if (createDescriptor)
//        {
//            var handle = _heaps[DescriptorHeapType.ShaderResourceView].AllocateTemp();
//            Unsafe.SkipInit(out D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc);
//            srvDesc.Format = DXGI_FORMAT.DXGI_FORMAT_UNKNOWN;
//            srvDesc.ViewDimension = D3D12_SRV_DIMENSION.D3D12_SRV_DIMENSION_BUFFER;
//            srvDesc.Shader4ComponentMapping = D3D12Constants.D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
//            srvDesc.Buffer.FirstElement = mappedMemory.ResourceOffset / stride; // uint32(tempMem.ResourceOffset / stride);
//            srvDesc.Buffer.Flags = D3D12_BUFFER_SRV_FLAGS.D3D12_BUFFER_SRV_FLAG_NONE;
//            srvDesc.Buffer.NumElements = count;
//            srvDesc.Buffer.StructureByteStride = stride;
//            _device.CreateShaderResourceView(_tempBuffers[_frameIndex].Resource, srvDesc, handle.CPU);
//            tempBuffer.DescriptorIndex = handle.Index;
//        }

//        return tempBuffer;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private MappedTempMemory GetTempMemory(uint size, uint alignment = 0)
//    {
//        var allocSize = size + alignment;
//        var offset = Interlocked.Add(ref _tempBuffers[_frameIndex].Offset, allocSize) - allocSize;
//        Debug.Assert(offset + size < _tempBufferSize);
//        if (alignment > 0)
//        {
//            // align the start offset to the stride of the requested size. 
//            offset = MemoryUtils.AlignNotPowerOf2(offset, alignment);
//        }

//        //NOTE(Jens): if we replace the buffers with a single resource (which we should), we need to update the offset to include the frame index
//        return new()
//        {
//            GPUAddress = _tempBuffers[_frameIndex].GPUAddress + offset,
//            CPUAddress = (byte*)_tempBuffers[_frameIndex].CPUAddress + offset,
//            ResourceOffset = offset
//        };
//    }

//    public void Update()
//    {
//        _frameIndex = (int)((_frameIndex + 1) % BufferCount);
//        _tempBuffers[_frameIndex].Offset = 0;

//        // only the SRV have temporary allocations, no need to call end frame on the other ones.
//        _heaps[DescriptorHeapType.ShaderResourceView].EndFrame();
//    }

//    public void Shutdown()
//    {
//        if (_memoryManager != null)
//        {
//            foreach (ref var tempBuffersDescriptor in _tempBuffers)
//            {
//                tempBuffersDescriptor.Resource.Dispose();
//                tempBuffersDescriptor = default;
//            }
//            for (var i = 0; i < (int)DescriptorHeapType.Count; ++i)
//            {
//                _heaps[i].Shutdown(_memoryManager);
//            }
//        }
//        _heaps = default;
//        _memoryManager = null;
//    }

//    [InlineArray((int)BufferCount)]
//    private struct TempBuffersDescriptors
//    {
//        private TempBufferDescriptor _ref;
//    }

//    [SkipLocalsInit]
//    private ref struct MappedTempMemory
//    {
//        public void* CPUAddress;
//        public ulong GPUAddress;
//        public uint ResourceOffset;
//    }
//}
