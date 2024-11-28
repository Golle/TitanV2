using System.Runtime.InteropServices;

namespace Titan.Tools.AssetProcessor.Processors.Textures.Aseprite;

[StructLayout(LayoutKind.Sequential, Size = 128, Pack = 2)]
internal unsafe struct AsepriteHeader
{
    public uint FileSize;
    public ushort MagicNumber;
    public ushort Frames;
    public ushort Width;
    public ushort Height;
    public ushort ColorDepth;
    public uint Flags;
    public ushort Speed;
    public uint Reserved1;
    public uint Reserved2;
    public byte PaletteEntry;
    public fixed byte Reserved3[3];
    public ushort NumberOfColors;
    public ushort PixelWidth;
    public ushort PixelHeight;
    public short GridX;
    public short GridY;
    public ushort GridWidth;
    public ushort GridHeight;
}