using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Input;
using Titan.Rendering.Resources;
using Titan.UI;
using Titan.UI.Resources;

namespace Titan.UI2;


public struct UIStyle
{
    public UIButtonStyle ButtonStyle;

}

public struct UIButtonStyle
{
    public AssetHandle<SpriteAsset> Asset;
    public AssetHandle<FontAsset> Font;
    public byte ButtonIndexStart;
}

/// <summary>
/// The UI Context
/// </summary>
public unsafe struct UIContext
{
    private readonly AssetsManager _assetsManager;
    private InputState* _inputState;
    private readonly UISystem2* _system;
    private readonly UIState* _state;
    private byte _contextId; // this should be used to uniquely identify a context.

    // We use a buffer of 32 widgets before flushing to the UI system
    private Inline32<UIWidget> _widgets;
    private int _count;
    private UIStyle* _style;
    private byte _layer;

    internal UIContext(AssetsManager assetsManager, InputState* inputState, UISystem2* system)
    {
        _assetsManager = assetsManager;
        _inputState = inputState;
        _system = system;
        _state = &_system->State;
    }

    public void Begin(byte layer)
        => Begin(_system->DefaultStyle, layer);

    public void Begin(in UIStyle style, byte layer)
    {
        _style = MemoryUtils.AsPointer(in style);
        _layer = layer;
    }

    public void End()
    {
        _system->Submit(_widgets.AsReadOnlySpan()[.._count]);
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddWidget(in UIWidget widget)
    {
        if (_count >= _widgets.Size)
        {
            End();
        }

        _widgets[_count++] = widget;
    }


    public bool Button(int id, ReadOnlySpan<byte> text, in Vector2 offset, in SizeF size, in Color color)
    {
        Debug.Assert(_style != null);

        return Button(id, text, offset, size, color, _style->ButtonStyle);
    }

    public bool Button(int id, ReadOnlySpan<byte> text, in Vector2 offset, in SizeF size, in Color color, in UIButtonStyle style)
    {
        if (!_assetsManager.IsLoaded(style.Asset) || !_assetsManager.IsLoaded(style.Font))
        {
            return false;
        }
        ref readonly var sprite = ref _assetsManager.Get(style.Asset);

        var isOver = MathUtils.IsWithin(offset, size, _state->CursorPosition);
        var c = color;
        var isClicked = false;
        var isDown = false;
        if (isOver)
        {
            SetHighlighted(id);
            if (IsHighlighted(id))
            {
                c = Color.Magenta;
            }

            //NOTE(Jens): This will check if the current element is highlighted, so if someone manages to click on the same frame as we highlight a button it wont register the click.
            if (ButtonPressed)
            {
                SetActive(id);
            }

            if (IsActive(id))
            {
                isClicked = ButtonReleaed;
                isDown = ButtonDown;
            }
        }

        if (isDown)
        {
            c = Color.Blue with { A = color.A };
        }

        AddWidget(UIWidget.Sprite(_layer, offset, size, sprite, c, style.ButtonIndexStart));

        return isClicked;
    }

    public void Sprite(in Vector2 offset, in SizeF size, AssetHandle<SpriteAsset> handle, byte index)
    {
        if (!_assetsManager.IsLoaded(handle))
        {
            return;
        }
        ref readonly var sprite = ref _assetsManager.Get(handle);
        var widget = UIWidget.Sprite(_layer, offset, size, sprite, Color.White, index);
        AddWidget(widget);
    }

    public void Image(in Vector2 offset, in SizeF scale, AssetHandle<TextureAsset> image)
    {


    }

    public void Box(in Vector2 offset, in SizeF size, in Color color)
    {
        var widget = new UIWidget
        {
            Layer = _layer,
            Size = size,
            Color = color,
            Offset = offset,
            Type = UIElementType.None,
        };
        AddWidget(widget);
    }



    private bool IsHighlighted(int id) => _state->HighlightedId == id;
    private bool IsActive(int id) => _state->ActiveId == id;
    private bool IsFocus(int id) => _state->FocusId == id;
    private void SetActive(int id) => _state->SetActive(id);
    private void SetHighlighted(int id) => _state->SetHighlighted(id, _layer);
    private bool ButtonPressed => _state->ButtonPressed;
    private bool ButtonReleaed => _state->ButtonReleased;
    private bool ButtonDown => _state->ButtonDown;
}
