using System.Diagnostics;
using System.Numerics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.UI.Text;

namespace Titan.UI.Resources;

[Asset(AssetType.Font)]
public partial struct FontAsset
{
    internal Inline256<Glyph> Glyphs;
    internal Handle<Texture> Sprite;
    internal int TextureId;
}

[AssetLoader<FontAsset>]
internal unsafe partial struct FontLoader
{
    // Need some better way to handle this. But this is probably fine for now. 
    private PoolAllocator<FontAsset> _assets;
    private D3D12ResourceManager* _resourceManager;
    private int _next;

    public bool Init(in AssetLoaderInitializer init)
    {
        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();
        var numberOfFonts = init.GetAssetCountByType(AssetType.Font);
        if (numberOfFonts == 0)
        {
            Logger.Warning<FontLoader>("No fonts have been registered. FontLoading disabled.");
            return true;
        }

        if (!init.MemoryManager.TryCreatePoolAllocator(out _assets, numberOfFonts))
        {
            Logger.Error<FontLoader>($"Failed to create the PoolAllocator. Count = {numberOfFonts}");
            return false;
        }
        return true;
    }

    public FontAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Debug.Assert(descriptor.Type == AssetType.Font);
        ref readonly var font = ref descriptor.Font;
        var glyphs = buffer.SliceArray<GlyphInfo>(0, descriptor.Font.NumberOfGlyphs);
        var glyphsSize = (uint)(descriptor.Font.NumberOfGlyphs * sizeof(GlyphInfo));
        var pixelSize = (uint)(font.BytesPerPixel * font.Width * font.Height);
        var pixels = buffer.Slice(glyphsSize, pixelSize);

        var asset = _assets.SafeAlloc();
        if (asset == null)
        {
            Logger.Error<FontLoader>("Failed to allocate a slot for the Font.");
            return null;
        }

        // set all slots to the default glyph
        var defaultGlyph = CreateGlyph(glyphs[font.DefaultGlyphIndex], font.Width, font.Height);
        for (var i = 0; i < asset->Glyphs.Size; ++i)
        {
            asset->Glyphs[i] = defaultGlyph;
        }

        // map the loaded glyphs to correct slot, based on the character index.
        foreach (ref readonly var glyph in glyphs.AsReadOnlySpan())
        {
            asset->Glyphs[glyph.Character] = CreateGlyph(glyph, font.Width, font.Height);
        }

        // load the sprite
        asset->Sprite = _resourceManager->CreateTexture(new CreateTextureArgs
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_R8_UNORM,
            Height = font.Height,
            Width = font.Width,
            ShaderVisible = true,
            InitialData = pixels
        });
        if (asset->Sprite.IsInvalid)
        {
            Logger.Error<FontLoader>("Failed to create the texture for font.");
            return null;
        }

        // cache the texture ID, this wont change unless its unloaded
        asset->TextureId = _resourceManager->Access(asset->Sprite)->SRV.Index;

        return asset;


        static Glyph CreateGlyph(in GlyphInfo info, uint width, uint height) =>
            new()
            {
                Width = info.Width,
                Height = info.Height,
                Advance = info.Width, // change this when we support it.
                Coords =
                {
                    UVMin = new Vector2(info.X, info.Y) / new Vector2(width, height),
                    UVMax = new Vector2(info.X + info.Width, info.Y + info.Height) / new Vector2(width, height)
                }
            };
    }

    public void Unload(FontAsset* asset)
    {
        Debug.Assert(asset != null);
        _resourceManager->DestroyTexture(asset->Sprite);
        _assets.SafeFree(asset);
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        // nyi

        Logger.Warning<FontLoader>("Shutdown - Not yet implemented.");
    }
}
