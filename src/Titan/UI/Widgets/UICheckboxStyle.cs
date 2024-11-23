using System.Runtime.CompilerServices;
using Titan.Assets;

namespace Titan.UI.Widgets;

public struct UICheckboxStyle
{
    public AssetHandle<Resources.SpriteAsset> CheckboxAsset;
    public byte UncheckedIndex;
    public byte CheckedIndex;
    public byte HoverIndex;
}

public struct UICheckboxState(UIID id)
{
    public readonly UIID Id = id;
    public bool IsChecked;

    public static UICheckboxState Create(bool isChecked = false) =>
        new(UIID.Create())
        {
            IsChecked = isChecked
        };

    public static void Create(Span<UICheckboxState> states)
    {
        foreach (ref var state in states)
        {
            state = Create();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(in UICheckboxState state) => state.IsChecked;
}
