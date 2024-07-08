using System.Runtime.InteropServices;

namespace Titan.Graphics.Pipeline.Graph;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RenderPassGroup(byte offset, byte count)
{
    public readonly byte Offset = offset;
    public readonly byte Count = count;
}
