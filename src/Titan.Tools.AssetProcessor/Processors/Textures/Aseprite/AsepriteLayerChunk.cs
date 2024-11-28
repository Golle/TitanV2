using System.Runtime.InteropServices;

namespace Titan.Tools.AssetProcessor.Processors.Textures.Aseprite;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct AsepriteLayerChunk
{
    public LayerFlags Flags;
    public ushort Type;
    public ushort ChildLevel;
    public ushort Width; // ignored
    public ushort Height; // ignored
    public BlendMode BlendMode;
    public byte Opacity;
    public fixed byte Reserved[3];
    // If type == 2
    public uint TilesetIndex;
}