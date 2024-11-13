using System.Runtime.InteropServices;

namespace Titan.UI.Text;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GlyphInfo
{
    public byte Character;
    public ushort X;
    public ushort Y;
    public byte Width;
    public byte Height;
}
