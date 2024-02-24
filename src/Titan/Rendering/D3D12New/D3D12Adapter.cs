using Titan.Core;
using Titan.Resources;

namespace Titan.Rendering.D3D12New;

[UnmanagedResource]
internal partial struct D3D12Adapter
{
    public const int MaxAdapters = 10;
    public uint PrimaryAdapterIndex;
    public uint AdapterCount;
    public Inline8<AdapterInfo> Adapters;
    public readonly ref readonly AdapterInfo PrimaryAdapter => ref Adapters[PrimaryAdapterIndex];
}
