using Titan.Assets;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

public struct UIRadioStyle
{
    public AssetHandle<SpriteAsset> AssetHandle;
    public byte Radio;
    public byte RadioSelected;
    public byte RadioHover;
}

public struct UIRadioState
{
    public byte SelectedIndex;
}
