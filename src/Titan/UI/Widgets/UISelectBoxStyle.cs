using Titan.Assets;
using Titan.UI.Resources;

namespace Titan.UI.Widgets;

public struct UISelectBoxState
{
    public byte SelectedIndex;
}

public struct UISelectBoxStyle
{
    public AssetHandle<SpriteAsset> SpriteHandle;
    public AssetHandle<FontAsset> FontHandle;
    public byte BackgroundIndex;
    public byte HoverIndex;
}
