using Titan.Assets;
using Titan.Core.Maths;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

public struct UISliderStyle
{
    public AssetHandle<SpriteAsset> AssetHandle;
    public byte BackgroundIndexLeft;
    public byte BackgroundIndexCenter;
    public byte BackgroundIndexRight;
    public byte BackgroundIndexEmptyLeft;
    public byte BackgroundIndexEmptyCenter;
    public byte BackgroundIndexEmptyRight;

    public byte SliderIndex;
    public byte SliderSelectedIndex;
    public SizeF SliderSize;
}

public struct UISliderState
{
    public float Value;
}

