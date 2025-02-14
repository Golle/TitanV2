using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Core.Maths;
using Titan.UI;
using Titan.UI.Resources;

namespace Titan.UI2;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct UIWidget
{
    public Color Color;
    public SizeF Size;
    public Vector2 Offset;
    public TextureCoordinate TextureCoordinates;
    public int TextureId;
    public UIElementType Type;
    public float Repeat;

    public ushort Id;
    private fixed byte Padding[2];

    public static UIWidget Sprite(ushort id, in Vector2 offset, in SizeF size, in SpriteAsset sprite, in Color color, byte index = 0)
    {
        Debug.Assert(index < sprite.Coordinates.Length);
        return new()
        {
            Id = id,
            TextureId = sprite.TextureId,
            Color = color,
            Type = UIElementType.Sprite,
            Offset = offset,
            Size = size,
            TextureCoordinates = sprite.Coordinates[index]
        };
    }

}
