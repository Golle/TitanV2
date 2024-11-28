using System.Runtime.InteropServices;

namespace Titan.Tools.AssetProcessor.Processors.Textures.Aseprite;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct AsepriteCelChunk
{
    public ushort LayerIndex;
    public short X;
    public short Y;
    public byte Opacity;
    public CelType Type;
    public fixed byte Reserved[7];
}