using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12.MemoryOld;

[SkipLocalsInit]
[DebuggerDisplay("CPU: {CPU.ptr, nq} GPU: {GPU.ptr, nq} Index: {Index, nq}")]
internal readonly struct TempDescriptorHandle(uint index, D3D12_CPU_DESCRIPTOR_HANDLE cpu, D3D12_GPU_DESCRIPTOR_HANDLE gpu)
{
    public readonly D3D12_CPU_DESCRIPTOR_HANDLE CPU = cpu;
    public readonly D3D12_GPU_DESCRIPTOR_HANDLE GPU = gpu;
    public readonly uint Index = index;
}
