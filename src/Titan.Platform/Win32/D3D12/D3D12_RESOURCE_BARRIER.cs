using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.D3D12;

public struct D3D12_RESOURCE_BARRIER
{
    public D3D12_RESOURCE_BARRIER_TYPE Type;
    public D3D12_RESOURCE_BARRIER_FLAGS Flags;
    private D3D12_RESOURCE_BARRIER_UNION _union;

    [UnscopedRef]
    public ref D3D12_RESOURCE_TRANSITION_BARRIER Transition => ref _union.Transition;
    [UnscopedRef]
    public ref D3D12_RESOURCE_ALIASING_BARRIER Aliasing => ref _union.Aliasing;
    [UnscopedRef] 
    public ref D3D12_RESOURCE_UAV_BARRIER UAV => ref _union.UAV;
    
    [StructLayout(LayoutKind.Explicit)]
    private struct D3D12_RESOURCE_BARRIER_UNION
    {
        [FieldOffset(0)]
        public D3D12_RESOURCE_TRANSITION_BARRIER Transition;
        [FieldOffset(0)]
        public D3D12_RESOURCE_ALIASING_BARRIER Aliasing;
        [FieldOffset(0)]
        public D3D12_RESOURCE_UAV_BARRIER UAV;
    }
}
