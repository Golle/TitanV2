using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
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

    public void ProgressBar(in Vector2 position, in SizeF size, ref UIProgressBarState state, in UIProgressBarStyle style)
    {
        if (!_assetsManager.IsLoaded(style.AssetHandle))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.AssetHandle);
        if (state.Value > 0.0f)
        {
            ref readonly var background = ref sprite.Coordinates[style.BackgroundIndex];
            var multiple = size.Width / (background.UVMax.X - background.UVMin.X);

            ref readonly var textureCoordinates = ref sprite.Coordinates[style.BarIndex];
            var barWidth = (textureCoordinates.UVMax.X - textureCoordinates.UVMin.X) * multiple;


            var sizeF = size with { Width = size.Width * state.Value };
            var repeat = sizeF.Width / barWidth;

            var barElement = new UIElement
            {
                TextureId = sprite.TextureId,
                TextureCoordinates = textureCoordinates,
                Type = UIElementType.SpriteRepeat,
                Size = sizeF,
                Offset = position,
                Repeat = repeat,
                Color = Color.White
            };

            _system->Add(barElement);
        }

        //NOTE(Jens): Render the progress bar first, and background on top to avoid calculating margins.
        _system->Add(new UIElement
        {
            TextureId = sprite.TextureId,
            TextureCoordinates = sprite.Coordinates[style.BackgroundIndex],
            Type = UIElementType.Sprite,
            Size = size,
            Offset = position,
            Color = Color.White
        });
    }

    public void SelectBox(in UIID id, in Vector2 position, in SizeF size, ReadOnlySpan<string> items, ref UISelectBoxState state, in UISelectBoxStyle style)
    {
        if (!_assetsManager.IsLoaded(style.SpriteHandle) || !_assetsManager.IsLoaded(style.FontHandle))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.SpriteHandle);
        ref readonly var font = ref _assetsManager.Get(style.FontHandle);
        var backgroundElement = new UIElement
        {
            TextureId = sprite.TextureId,
            TextureCoordinates = sprite.Coordinates[style.BackgroundIndex],
            Type = UIElementType.Sprite,
            Size = size,
            Offset = position,
            Color = Color.White
        };

        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);

        if (isOver && _inputState->IsButtonDown(MouseButton.Left))
        {
            _system->SetActive(id);
        }

        if (_system->IsActive(id))
        {
            var totalLength = 0;
            foreach (var item in items)
            {
                totalLength += item.Length;
            }

            //NOTE(Jens): this will not work when tehre are to many items. use a Bump Allocator or call system->ADd multiple times.
            Span<UIElement> elements = stackalloc UIElement[totalLength + items.Length];
            var offset = position with { Y = position.Y - 6 };

            var index = 0;
            for (var i = 0; i < items.Length; ++i)
            {
                var item = items[i];
                var isHighligthed = MathUtils.IsWithin(offset, size, _inputState->MousePositionUI);
                elements[index++] = backgroundElement with { Offset = offset, TextureCoordinates = sprite.Coordinates[isHighligthed ? style.HoverIndex : style.BackgroundIndex] };

                var textPosition = offset with { X = offset.X + 7 };
                InitTextW(elements[index..], textPosition, item, font, Color.White, 0.6f);
                index += item.Length;
                offset.Y -= (size.Height - 2);

                if (isHighligthed && _inputState->IsButtonReleased(MouseButton.Left))
                {
                    state.SelectedIndex = (byte)i;
                }
            }

            _system->Add(elements.Slice(0, index));

        }
        else
        {
            var item = items[state.SelectedIndex];
            Span<UIElement> elements = stackalloc UIElement[item.Length + 1];

            elements[0] = backgroundElement;
            var textPosition = new Vector2(position.X + 7, position.Y);
            InitTextW(elements[1..], textPosition, item, font, Color.White, 0.6f);
            _system->Add(elements);
        }

    }

    public void Radio(in UIID id, int index, in Vector2 position, in SizeF size, ref UIRadioState state, in UIRadioStyle style)
    {
        if (!_assetsManager.IsLoaded(style.AssetHandle))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.AssetHandle);
        var isOver = MathUtils.IsWithin(position, size, _inputState->MousePositionUI);
        if (isOver)
        {
            if (_inputState->IsButtonDown(MouseButton.Left))
            {
                _system->SetActive(id);
            }
            else if (_inputState->IsButtonReleased(MouseButton.Left))
            {
                state.SelectedIndex = (byte)index;
            }
        }

        var selected = state.SelectedIndex == index;
        var uiElement = new UIElement
        {
            TextureCoordinates = sprite.Coordinates[selected ? style.RadioSelected : style.Radio],
            Type = UIElementType.Sprite,
            Size = size,
            TextureId = sprite.TextureId,
            Offset = position,
            Color = Color.White
        };

        _system->Add(uiElement);

        if (isOver)
        {
            _system->Add(uiElement with
            {
                TextureCoordinates = sprite.Coordinates[style.RadioHover]
            });
        }
    }

    public void Slider(in UIID id, in Vector2 position, in SizeF size, ref UISliderState state, in UISliderStyle style)
    {
        if (!_assetsManager.IsLoaded(style.AssetHandle))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.AssetHandle);

        var sliderPosY = position.Y + (size.Height - style.SliderSize.Height) / 2f;
        var index = style.SliderIndex;
        var isActive = _system->IsActive(id);
        if (isActive)
        {
            var posX = _inputState->MousePositionUI.X;
            var diffX = Math.Clamp(posX - position.X, 0, size.Width);
            state.Value = diffX / size.Width;
            index = style.SliderSelectedIndex;
        }
        var value = Math.Clamp(state.Value, 0f, 1f);
        var sliderPosX = position.X + (size.Width * value) - style.SliderSize.Width / 2f;
        var sliderPos = new Vector2(sliderPosX, sliderPosY);
        if (!isActive)
        {
            var isOver = MathUtils.IsWithin(sliderPos, style.SliderSize, _inputState->MousePositionUI);
            if (isOver && _inputState->IsButtonDown(MouseButton.Left))
            {
                _system->SetActive(id);
            }
        }

        Span<UIElement> elements =
        [
            new()
            {
                Type = UIElementType.Sprite,
                Size = size,
                Color = Color.White,
                TextureCoordinates = sprite.Coordinates[style.BackgroundIndexCenter],
                TextureId = sprite.TextureId,
                Offset = position
            },
            new()
            {
                Type = UIElementType.Sprite,
                TextureCoordinates = sprite.Coordinates[index],
                Size = style.SliderSize,
                TextureId = sprite.TextureId,
                Offset = sliderPos,
                Color = Color.White
            }
        ];

        _system->Add(elements);
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
        InitText(elements, in position, text, in font, in color);
        _system->Add(elements);
    }

    private static void InitText(Span<UIElement> elements, in Vector2 position, ReadOnlySpan<byte> text, in FontAsset font, in Color color, float multiplier = 1.0f)
    {
        Debug.Assert(elements.Length >= text.Length);
        var offset = position;
        for (var i = 0; i < text.Length; ++i)
        {
            ref readonly var glyph = ref font.Glyphs[text[i]];
            elements[i] = new()
            {
                Color = color,
                Size = new SizeF(glyph.Width, glyph.Height) * multiplier,
                Offset = offset,
                TextureCoordinates = glyph.Coords,
                TextureId = font.TextureId,
                Type = UIElementType.Text
            };
            offset.X += glyph.Advance * multiplier;
        }
    }

    private static void InitTextW(Span<UIElement> elements, in Vector2 position, ReadOnlySpan<char> text, in FontAsset font, in Color color, float multiplier = 1.0f)
    {
        //NOTE(Jens): temp solution to support string. rework later
        Debug.Assert(elements.Length >= text.Length);
        var offset = position;
        for (var i = 0; i < text.Length; ++i)
        {
            ref readonly var glyph = ref font.Glyphs[(byte)text[i]];
            elements[i] = new()
            {
                Color = color,
                Size = new SizeF(glyph.Width, glyph.Height) * multiplier,
                Offset = offset,
                TextureCoordinates = glyph.Coords,
                TextureId = font.TextureId,
                Type = UIElementType.Text
            };
            offset.X += glyph.Advance * multiplier;
        }
    }


    public void Image(in Vector2 position, in SizeF size, in UIImageStyle style)
    {
        if (!_assetsManager.IsLoaded(style.Sprite))
        {
            return;
        }

        ref readonly var sprite = ref _assetsManager.Get(style.Sprite);


        if (style.IsNinePatch)
        {
            ref readonly var ninePatch = ref style.NinePatch;
            Span<UIElement> elements = stackalloc UIElement[9];
            var baseElement = new UIElement
            {
                Color = Color.White,
                TextureId = sprite.TextureId,
                Type = UIElementType.Sprite
            };


            //NOTE(Jens): A very naive way of doing 9 patch, this can be optimized a lot.
            /*
             * We're rendering 9 boxes, using 36 coordinates. It's possible to only use 8.
             * Positions can be shared, we would need shader support for that.
             * Sizes can be shared, only 4 insets and 2 sizes, we use 18 now.
             */
            var coordinateIndex = style.Index + 1;

            var x1 = position.X;
            var x2 = position.X + ninePatch.Left;
            var x3 = position.X + size.Width - ninePatch.Right;

            var y1 = position.Y;
            var y2 = position.Y + ninePatch.Bottom;
            var y3 = position.Y + size.Height - ninePatch.Top;

            var middleHeight = size.Height - ninePatch.Top - ninePatch.Bottom;
            var middleWidth = size.Width - ninePatch.Left - ninePatch.Right;

            elements[0] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex],
                Offset = new(x1, y1),
                Size = ninePatch.BottomLeft,
            };

            elements[1] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 1],
                Offset = new(x2, y1),
                Size = new(middleWidth, ninePatch.Bottom),
            };
            elements[2] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 2],
                Offset = new(x3, y1),
                Size = ninePatch.BottomRight,
            };

            elements[3] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 3],
                Offset = new(x1, y2),
                Size = new(ninePatch.Left, middleHeight),
            };
            elements[4] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 4],
                Offset = new(x2, y2),
                Size = new(middleWidth, middleHeight),
            };

            elements[5] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 5],
                Offset = new(x3, y2),
                Size = new(ninePatch.Right, middleHeight),
            };
            elements[6] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 6],
                Offset = new(x1, y3),
                Size = ninePatch.TopLeft,
            };

            elements[7] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 7],
                Offset = new(x2, y3),
                Size = new(middleWidth, ninePatch.Top),
            };
            elements[8] = baseElement with
            {
                TextureCoordinates = sprite.Coordinates[coordinateIndex + 8],
                Offset = new(x3, y3),
                Size = ninePatch.TopRight,
            };

            _system->Add(elements);
        }
        else
        {
            var element = new UIElement
            {
                Color = Color.White,
                Size = size,
                Offset = position,
                TextureCoordinates = sprite.Coordinates[style.Index],
                TextureId = sprite.TextureId,
                Type = UIElementType.Sprite
            };
            _system->Add(element);
        }

    }

    public void Image(in Vector2 position, in AssetHandle<Resources.SpriteAsset> handle, uint index = 0)
    {
        if (!_assetsManager.IsLoaded(handle))
        {
            return;
        }
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
            Span<UIElement> uiElements = stackalloc UIElement[count + 1];
            uiElements[0] = background;

            ref readonly var font = ref _assetsManager.Get(style.FontAsset);
            var offset = new Vector2(position.X + 10, position.Y + 4);
            InitText(uiElements[1..], offset, text[..count], font, Color.White, 0.5f);
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
