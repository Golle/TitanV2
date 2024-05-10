using System.Runtime.InteropServices;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct MeshDescriptor
{
    public int VertexCount;
    public int IndexCount;
    public int SubMeshCount;
    public int MaterialCount;
    //NOTE(Jens): Add additional configuration for Vertex format etc when we support other types. 
}
