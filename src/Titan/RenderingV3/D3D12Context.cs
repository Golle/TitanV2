using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;

namespace Titan.RenderingV3;

[UnmanagedResource]
internal partial struct D3D12Context
{
    public ComPtr<ID3D12Device4> Device;
    public D3D12Swapchain Swapchain;
    public D3D12CopyCommandLists CopyCommandLists;

    public Inline3<ComPtr<ID3D12CommandQueue>> CommandQueues;

    // max 3 command lists
    public Inline3<D3D12CommandLists> CommandLists;

    public Inline4<D3D12DescriptorHeap> DescriptorHeaps;

    public int FrameIndex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ID3D12CommandQueue* GetCommandQueue(CommandQueueTypes type)
        => CommandQueues[(int)type];

    public DescriptorHandle AllocDescriptor(DescriptorHeapTypes type)
        => DescriptorHeaps[(int)type].Alloc();

    public void FreeDescriptor(DescriptorHandle handle)
        => DescriptorHeaps[(int)handle.Type].Free(handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public D3D12_CPU_DESCRIPTOR_HANDLE GetCpuDescriptorHandle(DescriptorHandle handle)
        => DescriptorHeaps[(int)handle.Type].GetCpuDescriptorHandle(handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public D3D12_GPU_DESCRIPTOR_HANDLE GetGpuDescriptorHandle(DescriptorHandle handle)
        => DescriptorHeaps[(int)handle.Type].GetGpuDescriptorHandle(handle);


    [UnscopedRef]
    public ref D3D12CommandLists GetCurrentCommandList() => ref CommandLists[FrameIndex];

    public void Free() { }
}
