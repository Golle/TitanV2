using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Input;
using Titan.Rendering.Resources;
using Titan.UI;
using Titan.UI.Resources;

namespace Titan.UI2;

public struct UISelectBoxItemState
{
    internal SizeF Size;
    internal Vector2 Offset;
    internal UISelectBoxStyle2 Style;
}

public struct UISliderState2
{
    public float Value;
}



[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UICheckboxState2
{
    public bool Checked;
}


public struct UITextBoxStyle2
{
    public AssetHandle<SpriteAsset> Asset;
    public AssetHandle<FontAsset> Font;
    public byte Index;
    public byte FocusIndex;
    public byte CursorIndex;
}

public struct UISelectBoxStyle2
{
    public AssetHandle<SpriteAsset> Asset;
    public AssetHandle<FontAsset> Font;
    public byte Index;
    public byte FocusIndex;
    public byte ItemMargin;
}

public struct UIStyle
{
    public UIFontStyle Font;
    public UIButtonStyle Button;
    public UISliderStyle2 Slider;
    public UICheckboxStyle2 Checkbox;
    public UITextBoxStyle2 Textbox;
    public UISelectBoxStyle2 SelectBox;
}

public struct UIFontStyle
{
    public AssetHandle<FontAsset> Asset;
    public byte FontSize;
    public bool MonoFont;
}
public struct UICheckboxStyle2
{
    public AssetHandle<SpriteAsset> Asset;
    public byte Index;
    public byte SelectedIndex;
    public byte CheckmarkIndex;
}

public struct UISliderStyle2
{
    public AssetHandle<SpriteAsset> Asset;
    public byte BackgroundIndexLeft;
    public byte BackgroundIndexCenter;
    public byte BackgroundIndexRight;
    public byte BackgroundIndexEmptyLeft;
    public byte BackgroundIndexEmptyCenter;
    public byte BackgroundIndexEmptyRight;

    public byte SliderIndex;
    public byte SliderSelectedIndex;
    public SizeF SliderSize;

    public bool FillLeft;
}

public struct UIButtonStyle
{
    public AssetHandle<SpriteAsset> Asset;
    public byte ButtonIndexStart;
    public byte ButtonSelectedIndexStart;
    public byte ButtonDownIndexStart;
    public bool IsNinePatch;
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
    private ushort _nextId;

    public ref readonly UIStyle Style => ref *_style;

    internal UIContext(AssetsManager assetsManager, InputState* inputState, UISystem2* system)
    {
        _assetsManager = assetsManager;
        _inputState = inputState;
        _system = system;
        _state = &_system->State;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CursorInUI() => _state->HighlightedId != 0;
    public void Begin(byte layer)
        => Begin(_system->DefaultStyle, layer);

    public void Begin(in UIStyle style, byte layer)
    {
        _style = MemoryUtils.AsPointer(in style);
        _nextId = (ushort)(layer << 8);
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


    public bool Button(int id, ReadOnlySpan<char> text, in Vector2 offset, in SizeF size, in Color color, in Color textColor)
    {
        var clicked = Button(id, offset, size, color, _style->Button);
        var centerHeight = (int)size.Height >> 1;

        Label(new(offset.X + 10, offset.Y + centerHeight), size, text, in textColor);
        return clicked;
    }

    public bool Button(int id, in Vector2 offset, in SizeF size, in Color color)
    {
        Debug.Assert(_style != null);

        return Button(id, offset, size, color, _style->Button);
    }

    public bool Button(int id, in Vector2 offset, in SizeF size, in Color color, in UIButtonStyle style)
    {
        if (!_assetsManager.IsLoaded(style.Asset))
        {
            return false;
        }
        ref readonly var sprite = ref _assetsManager.Get(style.Asset);

        var isOver = IsOver(offset, size);
        var isClicked = false;
        var isDown = false;
        var isHighlighted = false;
        if (isOver)
        {
            SetHighlighted(id);
            isHighlighted = IsHighlighted(id);

            //NOTE(Jens): This will check if the current element is highlighted, so if someone manages to click on the same frame as we highlight a button it wont register the click.
            if (ButtonPressed)
            {
                SetActive(id);
            }

            if (IsActive(id))
            {
                isClicked = ButtonReleased;
                isDown = ButtonDown;
            }
        }

        var index = style.ButtonIndexStart;
        if (isDown)
        {
            index = style.ButtonDownIndexStart;
        }
        else if (isHighlighted)
        {
            index = style.ButtonSelectedIndexStart;
        }

        if (style.IsNinePatch)
        {
            DrawNinePatch(offset, size, color, sprite, index);
        }
        else
        {
            AddWidget(UIWidget.Sprite(NextId(), offset, size, sprite, in color, index));
        }


        return isClicked;
    }

    public void Sprite(in Vector2 offset, in SizeF size, AssetHandle<SpriteAsset> handle, byte index)
    {
        if (!_assetsManager.IsLoaded(handle))
        {
            return;
        }
        ref readonly var sprite = ref _assetsManager.Get(handle);
        AddWidget(UIWidget.Sprite(NextId(), offset, size, sprite, Color.White, index));
    }

    public void Image(in Vector2 offset, in SizeF scale, AssetHandle<TextureAsset> image)
    {


    }

    public void TextBox(int id, in Vector2 offset, in SizeF size, Span<char> text, ref int count)
        => TextBox(id, in offset, in size, text, ref count, _style->Textbox);

    public void TextBox(int id, in Vector2 offset, in SizeF size, Span<char> text, ref int count, in UITextBoxStyle2 style)
    {
        if (!_assetsManager.IsLoaded(style.Asset) || !_assetsManager.IsLoaded(style.Font))
        {
            return;
        }

        var isOver = IsOver(offset, size);
        var isFocus = IsFocus(id);
        if (isOver)
        {
            SetHighlighted(id);
            if (ButtonPressed)
            {
                SetActive(id);
            }
        }

        if (ButtonReleased && IsActive(id))
        {
            SetFocus(id);
        }

        if (isFocus && !isOver && ButtonPressed)
        {
            ClearFocus(id);
        }
        var isHighligted = isFocus || isOver;
        ref readonly var sprite = ref _assetsManager.Get(style.Asset);
        DrawNinePatch(offset, size, Color.White, sprite, isHighligted ? style.FocusIndex : style.Index);
        if (isFocus)
        {
            // Cursor
            var cursorOffset = offset + new Vector2(11, 9);
            if (count > 0)
            {
                ref readonly var font = ref _assetsManager.Get(style.Font);
                cursorOffset.X += count * font.Glyphs['A'].Advance;
            }
            AddWidget(UIWidget.Sprite(NextId(), cursorOffset, new(2, size.Height - 18), sprite, Color.White with { A = 0.8f }, style.CursorIndex));
        }

        if (count > 0)
        {
            ref readonly var font = ref _assetsManager.Get(style.Font);
            var fontHeight = font.Glyphs['A'].Height; // would be nice to have this in the font, LineHeight or something.
            var heightOffset = ((int)size.Height - fontHeight) >> 1;
            var textOffset = offset + new Vector2(10, heightOffset);
            DrawText16(textOffset, size with { Width = size.Width - 20 }, text[..count], Color.White, font);
        }

        if (isFocus)
        {
            foreach (var character in GetCharacters())
            {
                // Check for backspace
                if (character == 0x08)
                {
                    if (count > 0)
                    {
                        count--;
                    }
                    continue;
                }

                if (count >= text.Length)
                {
                    break;
                }

                text[count++] = character;
            }
        }
    }



    public void Box(in Vector2 offset, in SizeF size, in Color color, bool clickThrough = false)
    {
        var widget = new UIWidget
        {
            Id = NextId(),
            Size = size,
            Color = color,
            Offset = offset,
            Type = UIElementType.None,
        };
        AddWidget(widget);
    }

    public void Label(in Vector2 offset, in SizeF size, ReadOnlySpan<char> text, in Color color)
    {
        if (!_assetsManager.IsLoaded(_style->Font.Asset))
        {
            return;
        }

        ref readonly var font = ref _assetsManager.Get(_style->Font.Asset);
        DrawText16(offset, in size, text, in color, font);
    }

    public void Slider(int id, in Vector2 offset, in SizeF size, in Color color, ref UISliderState2 state)
    {
        if (!_assetsManager.IsLoaded(_style->Slider.Asset))
        {
            return;
        }

        ref readonly var style = ref _style->Slider;
        ref readonly var sprite = ref _assetsManager.Get(_style->Slider.Asset);

        var clampedStateValue = Math.Clamp(state.Value, 0.0f, 1.0f);
        var blobCenter = size.Width * clampedStateValue;

        var leftIndex = style.FillLeft ? style.BackgroundIndexEmptyLeft : style.BackgroundIndexLeft;
        var rightIndex = style.FillLeft ? style.BackgroundIndexRight : style.BackgroundIndexEmptyRight;
        var centerFilledIndex = style.FillLeft ? style.BackgroundIndexEmptyCenter : style.BackgroundIndexCenter;
        var centerEmptyIndex = style.FillLeft ? style.BackgroundIndexCenter : style.BackgroundIndexEmptyCenter;

        var sizes = sprite.Sizes;
        // Background
        {
            var height = sizes[centerEmptyIndex].Height;
            var heightOffset = ((int)size.Height - height) >> 1;
            var minWidth = sizes[leftIndex].Width + sizes[rightIndex].Width;
            var middleBarWidth = (int)size.Width - minWidth;
            var off = offset;
            off.Y += heightOffset;

            AddWidget(UIWidget.Sprite(NextId(), off, sizes[leftIndex], sprite, color, leftIndex));
            off.X += sizes[style.BackgroundIndexEmptyLeft].X;
            if (middleBarWidth > 0)
            {
                //NOTE(Jens): We could check for 0 and 1 and discard the part of the bar that is not drawn
                var barDiff = middleBarWidth * clampedStateValue;
                AddWidget(UIWidget.Sprite(NextId(), off, new(barDiff, height), sprite, color, centerFilledIndex));
                AddWidget(UIWidget.Sprite(NextId(), off with { X = off.X + barDiff }, new(middleBarWidth - barDiff, height), sprite, color, centerEmptyIndex));

                off.X += middleBarWidth;
            }
            AddWidget(UIWidget.Sprite(NextId(), off, sizes[rightIndex], sprite, color, rightIndex));
        }

        // Foreground (the blob)
        {
            var blobSize = sizes[style.SliderIndex];
            var off = offset;
            off.X += blobCenter - (blobSize.Width >> 1);
            off.Y += ((int)size.Height - blobSize.Height) >> 1;

            var isOver = IsOver(off, blobSize);
            if (isOver)
            {
                SetHighlighted(id);
                if (ButtonPressed)
                {
                    SetActive(id);
                }
            }

            // If it's active, update the state based on cursor position
            var isActive = IsActive(id);
            if (isActive)
            {
                var posX = CursorPosition.X;
                var diffX = Math.Clamp(posX - offset.X, 0, size.Width);

                state.Value = diffX / size.Width;
            }

            var selected = isActive || IsHighlighted(id);

            AddWidget(UIWidget.Sprite(NextId(), off, blobSize, sprite, color, selected ? style.SliderSelectedIndex : style.SliderIndex));
        }
    }

    private void DrawNinePatch(in Vector2 offset, in SizeF size, in Color color, in SpriteAsset sprite, byte startIndex)
    {
        Unsafe.SkipInit(out Inline3<float> widths);
        Unsafe.SkipInit(out Inline3<float> heights);

        widths[0] = sprite.Sizes[startIndex + 1].Width;
        widths[2] = sprite.Sizes[startIndex + 3].Width;
        widths[1] = (int)size.Width - (widths[0] + widths[2]);

        heights[0] = sprite.Sizes[startIndex + 1].Height;
        heights[2] = sprite.Sizes[startIndex + 7].Height;
        heights[1] = size.Height - (heights[0] + heights[2]);
        var index = (byte)(startIndex + 1);
        var off = offset;
        for (var y = 0; y < 3; ++y)
        {
            for (var x = 0; x < 3; ++x)
            {
                AddWidget(UIWidget.Sprite(NextId(), off, new(widths[x], heights[y]), sprite, color, index++));
                off.X += widths[x];
            }
            off.X = offset.X;
            off.Y += heights[y];
        }
    }

    private void DrawText16(in Vector2 offset, in SizeF size, ReadOnlySpan<char> text, in Color color, in FontAsset font)
    {
        var endX = offset.X + size.Width;
        var widget = new UIWidget
        {
            Id = NextId(),
            TextureId = font.TextureId,
            Color = color,
            Type = UIElementType.Text,
            Offset = offset
        };
        foreach (var character in text)
        {
            ref readonly var glyph = ref font.Glyphs[(byte)character];
            widget.Size = new(glyph.Width, glyph.Height);
            if (widget.Offset.X + glyph.Width > endX)
            {
                // cut of the text if outside the bounds
                break;
            }
            widget.TextureCoordinates = glyph.Coords;
            AddWidget(widget);

            widget.Offset.X += glyph.Advance;
        }
    }

    public void Checkbox(int id, in Vector2 offset, in SizeF size, in Color color, ref UICheckboxState2 state)
        => Checkbox(id, in offset, in size, color, ref state, _style->Checkbox);

    public void Checkbox(int id, in Vector2 offset, in SizeF size, in Color color, ref UICheckboxState2 state, in UICheckboxStyle2 style)
    {
        if (!_assetsManager.IsLoaded(style.Asset))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.Asset);
        var checkboxSize = sprite.Sizes[style.Index];
        var offsetX = ((int)size.Width - checkboxSize.Width) >> 1;
        var offsetY = ((int)size.Height - checkboxSize.Height) >> 1;
        var off = new Vector2(offset.X + offsetX, offset.Y + offsetY);

        if (IsOver(off, checkboxSize))
        {
            SetHighlighted(id);
            if (ButtonPressed)
            {
                SetActive(id);
            }

            if (IsActive(id) && ButtonReleased)
            {
                state.Checked = !state.Checked;
            }

            AddWidget(UIWidget.Sprite(NextId(), off, checkboxSize, sprite, color, style.SelectedIndex));
        }
        else
        {
            AddWidget(UIWidget.Sprite(NextId(), off, checkboxSize, sprite, color, style.Index));
        }


        if (state.Checked)
        {
            AddWidget(UIWidget.Sprite(NextId(), off, checkboxSize, sprite, color, style.CheckmarkIndex));
        }
    }


    public bool SelectBox(int id, in Vector2 offset, in SizeF size, out UISelectBoxItemState state)
    {
        return SelectBox(id, offset, size, out state, _style->SelectBox);
    }
    public bool SelectBox(int id, in Vector2 offset, in SizeF size, out UISelectBoxItemState state, in UISelectBoxStyle2 style)
    {
        Unsafe.SkipInit(out state);
        if (!_assetsManager.IsLoaded(style.Asset) || !_assetsManager.IsLoaded(style.Font))
        {
            return false;
        }

        var isOver = IsOver(offset, size);
        var isActive = IsActive(id);
        if (isOver)
        {
            SetHighlighted(id);
            if (ButtonPressed)
            {
                SetActive(id);
            }
        }

        var isHighligted = IsHighlighted(id) || isActive;

        ref readonly var sprite = ref _assetsManager.Get(style.Asset);
        DrawNinePatch(offset, size, Color.White, sprite, isHighligted ? style.FocusIndex : style.Index);

        if (isActive)
        {
            state = new()
            {
                Offset = offset with { Y = offset.Y - 10 },
                Size = size,
                Style = style
            };
        }
        return isActive;
    }


    public void SelectBoxItem(ref UISelectBoxItemState state, ReadOnlySpan<char> text)
    {
        var isOver = IsOver(state.Offset, state.Size);

        ref readonly var sprite = ref _assetsManager.Get(state.Style.Asset);
        ref readonly var font = ref _assetsManager.Get(state.Style.Font);

        //TODO(Jens): Implement LineHeight
        var textHeight = font.Glyphs['A'].Height;

        const float Margin = 6;
        var textOffsetY = ((int)state.Size.Height - textHeight) >> 1;
        var textOffset = new Vector2(state.Offset.X + Margin, state.Offset.Y + textOffsetY);
        var textSize = new SizeF(state.Size.Width, textHeight);

        DrawNinePatch(state.Offset, state.Size, Color.White, sprite, isOver ? state.Style.FocusIndex : state.Style.Index);
        DrawText16(textOffset, textSize, text, Color.White, _assetsManager.Get(state.Style.Font));

        state.Offset.Y -= (state.Size.Height + state.Style.ItemMargin);
    }

    private bool IsHighlighted(int id) => _state->HighlightedId == id;
    private bool IsActive(int id) => _state->ActiveId == id;
    private bool IsFocus(int id) => _state->FocusId == id;
    private void SetActive(int id) => _state->SetActive(id);
    private void SetHighlighted(int id) => _state->SetHighlighted(id, _layer);
    private void SetFocus(int id) => _state->SetFocus(id);
    private void ClearFocus(int id) => _state->ClearFocus(id);
    private bool ButtonPressed => _state->ButtonPressed;
    private bool ButtonReleased => _state->ButtonReleased;
    private bool ButtonDown => _state->ButtonDown;
    private ReadOnlySpan<char> GetCharacters() => _inputState->GetCharacters();

    private ref readonly Point CursorPosition => ref _state->CursorPosition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsOver(in Vector2 offset, in SizeF size) => MathUtils.IsWithin(offset, size, _state->CursorPosition);

    private ushort NextId() => _nextId++;
}
