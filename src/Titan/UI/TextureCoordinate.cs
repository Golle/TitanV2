using System.Numerics;
using System.Runtime.InteropServices;

namespace Titan.UI;

[StructLayout(LayoutKind.Sequential)]
public struct TextureCoordinate(in Vector2 min, in Vector2 max)
{
    public Vector2 UVMin = min;
    public Vector2 UVMax = max;
}


