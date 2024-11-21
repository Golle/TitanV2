using System.Numerics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core.Maths;
using Titan.Input;
using Titan.UI.Resources;
using Titan.UI.Widgets;

namespace Titan.UI;

/// <summary>
/// The public interface for managing UIs
/// </summary>
public readonly unsafe struct UIManager
{
    private readonly UISystem* _system;
    private readonly InputState* _inputState;
    private readonly AssetsManager _assetsManager;

    internal UIManager(UISystem* system, InputState* inputState, in AssetsManager assetsManager)
    {
        _system = system;
        _inputState = inputState;
        _assetsManager = assetsManager;
    }

    public void Box(in Vector2 position, in SizeF size, in Color color)
    {
        _system->Add(new UIElement
        {
            Color = color,
            Offset = position,
            Size = size,
            TextureCoordinates =
            {
                UVMin = Vector2.Zero,
                UVMax = Vector2.One
            }
        });
    }

    public bool Button(in UIID id, in Vector2 position, in SizeF size, in Color color)
    {
        var clicked = false;
        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);
        var isDown = false;
        var c = color;

        if (isOver)
        {
            var isHighlighted = _system->SetHighlighted(id);
            if (isHighlighted)
            {
                c = Color.Red with { A = color.A };
                if (_inputState->IsButtonPressed(MouseButton.Left))
                {
                    _system->SetActive(id);
                }

                if (_system->IsActive(id))
                {
                    isDown = _inputState->IsButtonDown(MouseButton.Left);
                    clicked = _inputState->IsButtonReleased(MouseButton.Left);
                }
            }
        }


        if (isDown)
        {
            c = Color.Blue with { A = color.A };
        }

        if (clicked)
        {
            c = Color.Magenta with { A = color.A };
        }

        _system->Add(new UIElement
        {
            Color = c,
            Offset = position,
            Size = size,
            TextureCoordinates =
            {
                UVMin = Vector2.Zero,
                UVMax = Vector2.One
            }
        });

        return clicked;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Text(in Vector2 position, ReadOnlySpan<byte> text, AssetHandle<FontAsset> fontHandle)
        => Text(position, text, fontHandle, Color.White);

    public void Text(in Vector2 position, ReadOnlySpan<byte> text, AssetHandle<FontAsset> fontHandle, in Color color)
    {
        if (!_assetsManager.IsLoaded(fontHandle))
        {
            return;
        }
        Text(position, text, _assetsManager.Get(fontHandle), color);
    }

    public void Text(in Vector2 position, ReadOnlySpan<byte> text, in FontAsset font)
        => Text(in position, text, font, Color.White);
    public void Text(in Vector2 position, ReadOnlySpan<byte> text, in FontAsset font, in Color color)
    {
        Span<UIElement> elements = stackalloc UIElement[text.Length];

        var offset = position;
        for (var i = 0; i < text.Length; ++i)
        {
            ref readonly var glyph = ref font.Glyphs[text[i]];
            elements[i] = new()
            {
                Color = color,
                Size = new(glyph.Width, glyph.Height),
                Offset = offset,
                TextureCoordinates = glyph.Coords,
                TextureId = font.TextureId,
                Type = UIElementType.Text
            };
            offset.X += glyph.Advance;
        }

        _system->Add(elements);
    }

    public void Image(in Vector2 position, in AssetHandle<Resources.SpriteAsset> handle, uint index = 0)
    {
        ref readonly var sprite = ref _assetsManager.Get(handle);
        var element = new UIElement
        {
            Color = Color.White,
            Size = new SizeF(189 * 2, 45 * 2),
            Offset = position,
            TextureCoordinates = sprite.Coordinates[index],
            TextureId = sprite.TextureId,
            Type = UIElementType.Sprite
        };
        _system->Add(element);
    }


    public bool Checkbox(in Vector2 position, in SizeF size, ref UICheckboxState state, in UICheckboxStyle style)
    {
        if (!_assetsManager.IsLoaded(style.CheckboxAsset))
        {
            return false;
        }

        var isDown = false;
        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);
        if (isOver)
        {
            var isHighlighted = _system->SetHighlighted(state.Id);
            if (isHighlighted)
            {
                if (_inputState->IsButtonPressed(MouseButton.Left))
                {
                    _system->SetActive(state.Id);
                }

                if (_system->IsActive(state.Id))
                {
                    isDown = _inputState->IsButtonDown(MouseButton.Left);
                    if (_inputState->IsButtonReleased(MouseButton.Left))
                    {
                        state.IsChecked = !state.IsChecked;
                    }
                }
            }
        }

        byte index;
        if (isDown)
        {
            index = style.HoverIndex;
        }
        else
        {
            index = state.IsChecked ? style.CheckedIndex : style.UncheckedIndex;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.CheckboxAsset);
        var element = new UIElement
        {
            Color = Color.White,
            Size = size,
            Offset = position,
            TextureCoordinates = sprite.Coordinates[index],
            TextureId = sprite.TextureId,
            Type = UIElementType.Sprite
        };
        _system->Add(element);

        return state.IsChecked;
    }

    public void TextBox(in UIID id, in Vector2 position, in SizeF size, Span<byte> text, in UITextBoxStyle style)
    {
        if (!_assetsManager.IsLoaded(style.FontAsset) || !_assetsManager.IsLoaded(style.SpriteAsset))
        {
            return;
        }

        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);
        var index = isOver ? style.SelectedIndex : style.DefaultIndex;
        ref readonly var sprite = ref _assetsManager.Get(style.SpriteAsset);

        var background = new UIElement
        {
            Color = Color.White,
            Size = size,
            Offset = position,
            TextureCoordinates = sprite.Coordinates[index],
            TextureId = sprite.TextureId,
            Type = UIElementType.Sprite
        };

        var count = GetNumberOfCharacters(text);

        foreach (var character in _inputState->GetCharacters())
        {
            if (character == '\b' && count > 0)
            {
                text[--count] = 0;
                continue;
            }
            if (count >= text.Length)
            {
                break;
            }
            text[count++] = (byte)character;
        }

        if (count > 0)
        {
            Span<UIElement> uiElements = new UIElement[count + 1];
            uiElements[0] = background;

            ref readonly var font = ref _assetsManager.Get(style.FontAsset);
            var offset = new Vector2(position.X + 10, position.Y + 4);
            for (var i = 0; i < count; ++i)
            {
                ref readonly var glyph = ref font.Glyphs[text[i]];
                uiElements[i + 1] = new()
                {
                    Color = Color.White,
                    Size = new(glyph.Width / 2f, glyph.Height / 2f),
                    Offset = offset,
                    TextureCoordinates = glyph.Coords,
                    TextureId = font.TextureId,
                    Type = UIElementType.Text
                };
                offset.X += glyph.Advance / 2f;
            }

            _system->Add(uiElements);
        }
        else
        {
            _system->Add(background);
        }

        static int GetNumberOfCharacters(ReadOnlySpan<byte> text)
        {
            for (var i = 0; i < text.Length; ++i)
            {
                if (text[i] == 0)
                {
                    return i;
                }
            }

            return text.Length;
        }
    }
}
