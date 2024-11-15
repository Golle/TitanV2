using System.Numerics;
using Titan.Assets;
using Titan.Core.Maths;
using Titan.Input;

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
            UVMin = Vector2.Zero,
            UVMax = Vector2.One
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
            UVMin = Vector2.Zero,
            UVMax = Vector2.One
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
                UVMin = glyph.UVMin,
                UVMax = glyph.UVMax,
                TextureId = font.TextureId
            };
            offset.X += glyph.Advance;
        }

        _system->Add(elements);
    }
}


