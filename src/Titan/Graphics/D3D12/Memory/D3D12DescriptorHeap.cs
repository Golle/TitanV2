using Titan.Core;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12.Memory;

internal struct D3D12DescriptorHeap
{
    public ComPtr<ID3D12DescriptorHeap> Heap;
    public int Count;
    public uint NumberOfDescriptors;
    public uint NumberOfTempDescriptors;
    public uint TempBufferOffset;
    public uint IncrementSize;
    public D3D12_CPU_DESCRIPTOR_HANDLE CPUStart;
    public D3D12_GPU_DESCRIPTOR_HANDLE GPUStart;
    public TitanArray<int> FreeList;
    public bool ShaderVisible;
}
