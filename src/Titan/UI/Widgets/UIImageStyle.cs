using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core.Maths;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UIImageStyle
{
    public AssetHandle<SpriteAsset> Sprite;
    public UIImageStyleNinePatch NinePatch;
    public byte Index;
    public bool IsNinePatch;
}

public struct UIImageStyleNinePatch(byte left, byte top, byte right, byte bottom)
{
    public byte Left = left;
    public byte Top = top;
    public byte Right = right;
    public byte Bottom = bottom;
    public SizeF BottomLeft => new(Left, Bottom);
    public SizeF BottomRight => new(Right, Bottom);
    public SizeF TopLeft => new(Left, Top);
    public SizeF TopRight => new(Right, Top);


    public static UIImageStyleNinePatch FromUint(uint value) =>
        new()
        {
            Left = (byte)(value & 0xff),
            Top = (byte)(value >> 8 & 0xff),
            Right = (byte)(value >> 16 & 0xff),
            Bottom = (byte)(value >> 24 & 0xff)
        };
    public static UIImageStyleNinePatch FromValue(byte value) =>
        new()
        {
            Left = value,
            Top = value,
            Right = value,
            Bottom = value
        };
}
