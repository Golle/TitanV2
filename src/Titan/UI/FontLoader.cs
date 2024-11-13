using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.UI.Text;

namespace Titan.UI;

[Asset(AssetType.Font)]
public partial struct FontAsset
{
    internal int Index;
    internal Inline256<Glyph> Glyphs;
    internal Handle<Texture> Sprite;
}

[AssetLoader<FontAsset>]
internal unsafe partial struct FontLoader
{
    // Need some better way to handle this. But this is probably fine for now. 
    private Inline10<FontAsset> _assets;
    private GeneralAllocator _allocator;
    private D3D12ResourceManager* _resourceManager;
    private UISystem* _uiSystem;

    private int _next;

    private SpinLock _lock;

    public bool Init(in AssetLoaderInitializer init)
    {
        var MaxGlyphMemory = MemoryUtils.MegaBytes(2);
        var preAllocatedSize = MemoryUtils.KiloBytes(64);
        if (!init.MemoryManager.TryCreateGeneralAllocator(out _allocator, MaxGlyphMemory, preAllocatedSize))
        {
            Logger.Error<FontLoader>($"Failed to create a general allocator. Size = {MaxGlyphMemory}, PreAllocated = {preAllocatedSize}");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();
        _uiSystem = init.GetResourcePointer<UISystem>();
        return true;
    }

    public FontAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        using var _ = new MeasureTime<FontLoader>("Font loaded in {0} ms.");
        Debug.Assert(descriptor.Type == AssetType.Font);
        ref readonly var font = ref descriptor.Font;
        var glyphs = buffer.SliceArray<GlyphInfo>(0, descriptor.Font.NumberOfGlyphs);
        var glyphsSize = (uint)(descriptor.Font.NumberOfGlyphs * sizeof(GlyphInfo));
        var pixelSize = (uint)(font.BytesPerPixel * font.Width * font.Height);
        var pixels = buffer.Slice(glyphsSize, pixelSize);

        var index = GetSlot();
        Debug.Assert(index < _assets.Size);

        var asset = _assets.GetPointer(index);
        asset->Index = index;

        // set all slots to the default glyph
        var defaultGlyph = CreateGlyph(glyphs[font.DefaultGlyphIndex]);
        for (var i = 0; i < asset->Glyphs.Size; ++i)
        {
            asset->Glyphs[i] = defaultGlyph;
        }

        // map the loaded glyphs to correct slot, based on the character index.
        foreach (ref readonly var glyph in glyphs.AsReadOnlySpan())
        {
            asset->Glyphs[glyph.Character] = CreateGlyph(glyph);
        }

        // load the sprite
        asset->Sprite = _resourceManager->CreateTexture(new CreateTextureArgs
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UINT,
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

        // upload the glyph to the slot
        _uiSystem->UploadFont(index, asset->Glyphs);

        return asset;


        static Glyph CreateGlyph(in GlyphInfo info) =>
            new()
            {
                Advance = info.Width, // change this when we support it.
                MaxUV = new(info.X + info.Width, info.Y + info.Height),
                MinUV = new(info.X, info.Y),
            };
    }

    public void Unload(FontAsset* asset)
    {
        Debug.Assert(asset != null);
        _resourceManager->DestroyTexture(asset->Sprite);
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        // nyi

        Logger.Warning<FontLoader>("Shutdown - Not yet implemented.");
    }

    private int GetSlot()
    {
        var index = Interlocked.Increment(ref _next) - 1;
        Debug.Assert(index < _assets.Size, "Out of slots.");
        return index;
    }


    private TitanArray<T> AllocArray<T>(uint count) where T : unmanaged
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        var array = _allocator.AllocArray<T>(count);
        _lock.Exit();
        return array;
    }

    private void FreeArray<T>(ref TitanArray<T> array) where T : unmanaged
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        _allocator.FreeArray(ref array);
        _lock.Exit();
    }
}
