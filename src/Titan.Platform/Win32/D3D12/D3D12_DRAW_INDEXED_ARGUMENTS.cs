using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.D3D12;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct D3D12_DRAW_INDEXED_ARGUMENTS
{
    public uint IndexCountPerInstance;
    public uint InstanceCount;
    public uint StartIndexLocation;
    public int BaseVertexLocation;
    public uint StartInstanceLocation;
}
