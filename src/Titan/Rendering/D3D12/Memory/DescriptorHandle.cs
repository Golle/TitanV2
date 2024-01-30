using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Rendering.D3D12.Memory;

[DebuggerDisplay("CPU: {CPU.ptr, nq} GPU: {GPU.ptr, nq}")]
internal readonly struct DescriptorHandle
{
    public readonly D3D12_CPU_DESCRIPTOR_HANDLE CPU;
    public readonly D3D12_GPU_DESCRIPTOR_HANDLE GPU;
    public readonly uint Index;
    public readonly DescriptorHeapType Type;

    public DescriptorHandle(DescriptorHeapType type, D3D12_CPU_DESCRIPTOR_HANDLE cpu, D3D12_GPU_DESCRIPTOR_HANDLE gpu, uint index)
    {
        Debug.Assert(cpu.ptr != 0, "cpu.ptr != 0, this is not expected since that is how we check for an invalid handle. Rework this.");
        Type = type;
        CPU = cpu;
        GPU = gpu;
        Index = index;
    }

    public bool IsValid => CPU.ptr != 0;
    public bool IsShaderVisible => GPU.ptr != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator D3D12_CPU_DESCRIPTOR_HANDLE(in DescriptorHandle handle) => handle.CPU;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator D3D12_GPU_DESCRIPTOR_HANDLE(in DescriptorHandle handle) => handle.GPU;
}
