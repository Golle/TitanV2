using System.Runtime.CompilerServices;

namespace Titan.Rendering.D3D12.Memory;

[SkipLocalsInit]
internal unsafe struct TempConstantBuffer
{
    public void* CPUAddress;
    public ulong GPUAddress;
    public uint Size;
}
