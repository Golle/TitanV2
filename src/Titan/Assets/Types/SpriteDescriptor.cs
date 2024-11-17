using System.Runtime.InteropServices;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct SpriteDescriptor
{
    public Texture2DDescriptor Texture;
    public byte NumberOfSprites;
}
