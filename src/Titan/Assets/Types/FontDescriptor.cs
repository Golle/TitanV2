using System.Runtime.InteropServices;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential)]
public struct FontDescriptor
{
    public ushort NumberOfGlyphs;
    public ushort DefaultGlyphIndex;
    public ushort Width;
    public ushort Height;
    public byte BytesPerPixel;
}
