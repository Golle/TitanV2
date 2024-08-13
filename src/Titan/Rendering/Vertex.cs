using System.Numerics;
using System.Runtime.InteropServices;

namespace Titan.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector3 Position;
    public Vector2 UV;
}
