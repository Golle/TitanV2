using Titan.Assets;

namespace Titan.UI.Widgets;

public struct UITextBoxStyle
{
    public AssetHandle<Resources.FontAsset> FontAsset;
    public AssetHandle<Resources.SpriteAsset> SpriteAsset;
    public byte DefaultIndex;
    public byte SelectedIndex;

}
