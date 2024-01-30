using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Rendering.D3D12.Memory;

[InlineArray((int)DescriptorHeapType.Count)]
internal struct DescriptorHeaps
{
    private DescriptorHeap _ref;
    public unsafe ref DescriptorHeap this[DescriptorHeapType type]
    {
        get
        {
            Debug.Assert(type != DescriptorHeapType.Count);
            return ref *((DescriptorHeap*)Unsafe.AsPointer(ref _ref) + (int)type);
        }
    }
}
