using Titan.Assets;
using Titan.Core.Maths;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

public struct UISliderStyle
{
    public AssetHandle<SpriteAsset> AssetHandle;
    public byte BackgroundIndex;
    public byte SliderIndex;
    public byte SliderSelectedIndex;
    public SizeF SliderSize;
}

public struct UISliderState
{
    public float Value;
}

