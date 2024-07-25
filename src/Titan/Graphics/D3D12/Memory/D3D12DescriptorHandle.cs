using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12.Memory;

[DebuggerDisplay("CPU: {CPU.ptr, nq} GPU: {GPU.ptr, nq}")]
internal readonly struct D3D12DescriptorHandle(DescriptorHeapType type, D3D12_CPU_DESCRIPTOR_HANDLE cpu, D3D12_GPU_DESCRIPTOR_HANDLE gpu, int index)
{
    public readonly D3D12_CPU_DESCRIPTOR_HANDLE CPU = cpu;
    public readonly D3D12_GPU_DESCRIPTOR_HANDLE GPU = gpu;
    public readonly int Index = index;
    public readonly DescriptorHeapType Type = type;

    public bool IsValid => CPU.ptr != 0;
    public bool IsShaderVisible => GPU.ptr != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator D3D12_CPU_DESCRIPTOR_HANDLE(in D3D12DescriptorHandle handle) => handle.CPU;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator D3D12_GPU_DESCRIPTOR_HANDLE(in D3D12DescriptorHandle handle) => handle.GPU;
}
