using System.Runtime.InteropServices;

namespace Titan.Tools.AssetProcessor.Processors.Textures.Aseprite;

[StructLayout(LayoutKind.Sequential, Size = 16)]
internal unsafe struct AsepriteFrame
{
    public uint Size;
    public ushort MagicNumber;
    public ushort NumberOfChunksOld;
    public ushort FrameDurationMs;
    public fixed byte NotUsed[2];
    public uint NumberOfChunks;
}