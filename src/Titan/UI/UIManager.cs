using System;
using System.Numerics;
using System.Reflection;
using Titan.Assets;
using Titan.Core.Maths;
using Titan.Input;
using static Titan.Assets.EngineAssetsRegistry;

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

    public bool Button(in Vector2 position, in SizeF size, in Color color)
    {
        var clicked = false;
        var id = _system->GetNextId();
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

    public void Text(in Vector2 position, ReadOnlySpan<byte> text, AssetHandle<FontAsset> fontHandle)
    {
        if (!_assetsManager.IsLoaded(fontHandle))
        {
            return;
        }
        Text(position, text, _assetsManager.Get(fontHandle));
    }

    public void Text(in Vector2 position, ReadOnlySpan<byte> text, in FontAsset font)
    {
        Span<UIElement> elements = stackalloc UIElement[text.Length];

        var offset = position;
        for (var i = 0; i < text.Length; ++i)
        {
            ref readonly var glyph = ref font.Glyphs[text[i]];
            elements[i] = new()
            {
                Color = Color.White,
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

    public void Image(in Vector2 position, in AssetHandle<SpriteAsset> handle, uint index = 0)
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


    public bool Checkbox(in Vector2 position, in SizeF size, ref bool isChecked, in UICheckboxStyle style)
    {
        if (!_assetsManager.IsLoaded(style.CheckboxAsset))
        {
            return false;
        }

        var isDown = false;
        var id = _system->GetNextId();
        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);
        if (isOver)
        {
            var isHighlighted = _system->SetHighlighted(id);
            if (isHighlighted)
            {
                if (_inputState->IsButtonPressed(MouseButton.Left))
                {
                    _system->SetActive(id);
                }

                if (_system->IsActive(id))
                {
                    isDown = _inputState->IsButtonDown(MouseButton.Left);
                    if (_inputState->IsButtonReleased(MouseButton.Left))
                    {
                        isChecked = !isChecked;
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
            index = isChecked ? style.CheckedIndex : style.UncheckedIndex;
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

        return isChecked;
    }
}

public struct UICheckboxStyle
{
    public AssetHandle<SpriteAsset> CheckboxAsset;
    public byte UncheckedIndex;
    public byte CheckedIndex;
    public byte HoverIndex;
}


