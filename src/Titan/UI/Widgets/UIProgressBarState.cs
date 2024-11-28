using Titan.Assets;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

public struct UIProgressBarState
{
    public float Value;
}

public struct UIProgressBarStyle
{
    public AssetHandle<SpriteAsset> AssetHandle;
    public byte BackgroundIndex;
    public byte BarIndex;
}
