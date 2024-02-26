using Titan.Core;
using Titan.Resources;

namespace Titan.Rendering.D3D12New.Memory;

[UnmanagedResource]
internal partial struct D3D12DescriptorHeaps
{
    public TitanArray<int> SharedFreeList;
    public Inline4<DescriptorHeap> Heaps;
}
