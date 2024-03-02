using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Memory;

namespace Titan.Graphics.D3D12.MemoryOld;

[SkipLocalsInit]
internal unsafe struct StructuredBuffer<T> where T : unmanaged
{
    private static readonly uint Stride = (uint)sizeof(T);
    public void* CPUAddress;
    public ulong GPUAddress;
    public uint Count;
    public uint DescriptorIndex;

    public void Copy(T* data, uint count, uint offset = 0)
    {
        Debug.Assert(offset + count <= Count);
        Debug.Assert(CPUAddress != null);

        MemoryUtils.Copy((byte*)CPUAddress + offset * Stride, data, Stride * count);
    }
    public void Copy(ReadOnlySpan<T> data, uint offset = 0)
    {
        Debug.Assert(data.Length + offset <= Count);
        Debug.Assert(CPUAddress != null);
        MemoryUtils.Copy((byte*)CPUAddress + offset * Stride, data);
    }
}
