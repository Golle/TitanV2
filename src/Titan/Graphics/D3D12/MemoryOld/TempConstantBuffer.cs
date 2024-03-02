using System.Runtime.CompilerServices;

namespace Titan.Graphics.D3D12.MemoryOld;

[SkipLocalsInit]
internal unsafe struct TempConstantBuffer
{
    public void* CPUAddress;
    public ulong GPUAddress;
    public uint Size;
}
