using System.Runtime.InteropServices;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct MeshDescriptor
{
    public uint VertexCount;
    public uint SubMeshCount;
    public uint IndexCount;
    public int MaterialCount;
    //NOTE(Jens): Add additional configuration for Vertex format etc when we support other types. 
}
