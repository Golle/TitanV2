using System.Numerics;
using System.Runtime.InteropServices;

namespace Titan.Rendering;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vertex
{
    public Vector3 Position;
    public Vector2 UV;
    public Vector3 Normal;
}


[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionNormal
{
    public Vector3 Position;
    public Vector3 Normal;
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionUV
{
    public Vector3 Position;
    public Vector2 UV;
}
