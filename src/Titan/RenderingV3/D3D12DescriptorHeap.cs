using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.RenderingV3;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public record struct DescriptorHandle(ushort Index, DescriptorHeapTypes Type)
{
    public bool IsValid => Index > 0;
}

internal struct D3D12DescriptorHeap
{
    // D3D12 specific
    public ComPtr<ID3D12DescriptorHeap> Resource;
    public D3D12_CPU_DESCRIPTOR_HANDLE CPUStart;
    public D3D12_GPU_DESCRIPTOR_HANDLE GPUStart;
    // End D3D12 specific

    public uint IncrementSize;
    private SpinLock _lock;
    public DescriptorHeapTypes Type;
    public bool ShaderVisibile;
    public ushort MaxCount;
    public ushort Count;
    public Inline1024<ushort> FreeList;

    public DescriptorHandle Alloc()
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        Debug.Assert(Count < MaxCount);
        var index = Count++;
        var descriptorIndex = FreeList[index];
        _lock.Exit();
        return new DescriptorHandle(descriptorIndex, Type);
    }

    public void Free(DescriptorHandle handle)
    {
        Debug.Assert(handle.Type == Type);
        var gotLock = false;
        _lock.Enter(ref gotLock);
        var index = --Count;
        FreeList[index] = handle.Index;
        _lock.Exit();
        CheckForDuplicates();
    }

    public D3D12_CPU_DESCRIPTOR_HANDLE GetCpuDescriptorHandle(DescriptorHandle handle)
    {
        Debug.Assert(handle.Type == Type);
        var offset = CPUStart;
        offset.ptr += handle.Index * IncrementSize;
        return offset;
    }

    public D3D12_GPU_DESCRIPTOR_HANDLE GetGpuDescriptorHandle(DescriptorHandle handle)
    {
        Debug.Assert(handle.Type == Type);
        Debug.Assert(ShaderVisibile, $"The descriptor heap is not shader visible. Type = {Type}");

        var offset = GPUStart;
        offset.ptr += handle.Index * IncrementSize;
        return offset;
    }

    [Conditional("DEBUG")]
    private void CheckForDuplicates()
    {
        for (var i = 0; i < Count - 1; ++i)
        {
            for (var j = i + 1; j < Count; ++j)
            {
                Debug.Assert(FreeList[i] != FreeList[j], "Duplicated descriptor handles.");
            }
        }
    }
}
