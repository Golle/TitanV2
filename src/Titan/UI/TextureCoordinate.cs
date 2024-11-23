using System.Numerics;
using System.Runtime.InteropServices;

namespace Titan.UI;

[StructLayout(LayoutKind.Sequential)]
public struct TextureCoordinate
{
    public Vector2 UVMin;
    public Vector2 UVMax;
}
