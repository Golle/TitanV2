using System.Runtime.InteropServices;

namespace Titan.Tools.AssetProcessor.Parsers.Ogg;

[StructLayout(LayoutKind.Explicit)]
internal struct VorbisFloorConfig
{
    [FieldOffset(0)]
    public int Type;
    [FieldOffset(sizeof(int))]
    public VorbisFloorConfig0 Config0;
    [FieldOffset(sizeof(int))]
    public VorbisFloorConfig1 Config1;
}
