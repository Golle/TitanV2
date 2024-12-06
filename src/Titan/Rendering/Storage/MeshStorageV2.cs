using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Core.Maths;

namespace Titan.Rendering.Storage;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MeshInstanceData
{
    public Matrix4x4 Transform;
    public uint MaterialId;
}

