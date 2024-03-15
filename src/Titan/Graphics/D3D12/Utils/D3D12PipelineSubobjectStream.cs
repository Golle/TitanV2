using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Memory;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using static Titan.Platform.Win32.D3D12.D3D12_PIPELINE_STATE_SUBOBJECT_TYPE;

namespace Titan.Graphics.D3D12.Utils;

[StructLayout(LayoutKind.Sequential, Size = MaxSize)]
public unsafe ref struct D3D12PipelineSubobjectStream
{
    public const int MaxSize = 1024;
    private uint _offset;
    private byte _bufferStart;

    public D3D12PipelineSubobjectStream VS(D3D12_SHADER_BYTECODE byteCode) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_VS, byteCode);
    public D3D12PipelineSubobjectStream PS(D3D12_SHADER_BYTECODE byteCode) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_PS, byteCode);
    public D3D12PipelineSubobjectStream RootSignature(ID3D12RootSignature* rootSignature) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_ROOT_SIGNATURE, rootSignature);
    public D3D12PipelineSubobjectStream Topology(D3D12_PRIMITIVE_TOPOLOGY_TYPE topology) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_PRIMITIVE_TOPOLOGY, topology);
    public D3D12PipelineSubobjectStream Razterizer(in D3D12_RASTERIZER_DESC desc) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_RASTERIZER, desc);
    public D3D12PipelineSubobjectStream Blend(in D3D12_BLEND_DESC blend) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_BLEND, blend);
    public D3D12PipelineSubobjectStream DepthStencil(in D3D12_DEPTH_STENCIL_DESC depth) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_DEPTH_STENCIL, depth);
    public D3D12PipelineSubobjectStream SampleMask(uint sampleMask) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_SAMPLE_MASK, sampleMask);
    public D3D12PipelineSubobjectStream RenderTargetFormat(in D3D12_RT_FORMAT_ARRAY format) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_RENDER_TARGET_FORMATS, format);
    public D3D12PipelineSubobjectStream Sample(in DXGI_SAMPLE_DESC sample) => Add(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE_SAMPLE_DESC, sample);

    private D3D12PipelineSubobjectStream Add<T>(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE type, T* data) where T : unmanaged => Add(new PointerType<T>(type, data));
    private D3D12PipelineSubobjectStream Add<T>(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE type, T data) where T : unmanaged => Add(new DataType<T>(type, data));
    private D3D12PipelineSubobjectStream Add<T>(T value) where T : unmanaged
    {
        _offset = MemoryUtils.AlignToUpper(_offset);
        Debug.Assert(_offset + sizeof(T) <= MaxSize, "The buffer to store the pipeline stream is to small.");

        *(T*)(GetBuffer() + _offset) = value;
        _offset += (uint)sizeof(T);
        return this;
    }

    private byte* GetBuffer()
        => (byte*)Unsafe.AsPointer(ref _bufferStart);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct DataType<T>(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE type, in T value)
        where T : unmanaged
    {
        private readonly D3D12_PIPELINE_STATE_SUBOBJECT_TYPE _type = type;
        private readonly T _value = value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct PointerType<T>(D3D12_PIPELINE_STATE_SUBOBJECT_TYPE type, T* value)
        where T : unmanaged
    {
        private readonly D3D12_PIPELINE_STATE_SUBOBJECT_TYPE _type = type;
        private readonly T* _value = value;
    }

    public D3D12_PIPELINE_STATE_STREAM_DESC AsStreamDesc() =>
        new()
        {
            SizeInBytes = MemoryUtils.AlignToUpper(_offset),
            pPipelineStateSubobjectStream = GetBuffer()
        };
}
