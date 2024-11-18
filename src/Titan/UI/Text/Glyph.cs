using System.Numerics;
using System.Runtime.InteropServices;

namespace Titan.UI.Text;

[StructLayout(LayoutKind.Sequential)]
internal struct Glyph
{
    public TextureCoordinate Coords;
    public uint Advance;
    public ushort Width;
    public ushort Height;
}
