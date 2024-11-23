using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Rendering;

namespace Titan.UI.Resources;


[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct SpriteInfo
{
    public ushort MinX;
    public ushort MinY;
    public ushort MaxX;
    public ushort MaxY;
}

[Asset(AssetType.Sprite)]
public partial struct SpriteAsset
{
    public Handle<Texture> Texture;
    public int TextureId;
    public TitanArray<TextureCoordinate> Coordinates;
}

[AssetLoader<SpriteAsset>]
internal unsafe partial struct SpriteLoader
{
    private static readonly Lock _lock = new();
    private D3D12ResourceManager* _resourceManager;
    private GeneralAllocator _allocator;
    private PoolAllocator<SpriteAsset> _assets;

    public bool Init(in AssetLoaderInitializer init)
    {
        //TODO(Jens): Can we be even smarter with this? We know everything about all assets at startup, we could set up the memory usage for this as well. Might require more metadata.
        var maxSpriteCount = init.GetAssetCountByType(AssetType.Sprite);
        Logger.Trace<SpriteLoader>($"Registered Sprites = {maxSpriteCount}");
        if (maxSpriteCount == 0)
        {
            Logger.Warning<SpriteLoader>("No sprites in asset registries, Sprite Loader disabled.");
            return true;
        }
        if (!init.MemoryManager.TryCreatePoolAllocator(out _assets, maxSpriteCount))
        {
            Logger.Error<SpriteLoader>($"Failed to create the pool allocator. Count = {maxSpriteCount} Size = {sizeof(SpriteAsset) * maxSpriteCount}");
            return false;
        }

        //TODO(Jens): This allocater can probably be modified a bit, so it bases the memory usage on the number of assets and their size.
        if (!init.MemoryManager.TryCreateGeneralAllocator(out _allocator, MemoryUtils.MegaBytes(1), MemoryUtils.MegaBytes(32)))
        {
            Logger.Error<SpriteLoader>("Failed to create the general allocator.");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();

        return true;
    }

    public SpriteAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Debug.Assert(descriptor.Type == AssetType.Sprite);
        ref readonly var spriteDescriptor = ref descriptor.Sprite;
        ref readonly var texture = ref spriteDescriptor.Texture;

        var sprites = buffer.SliceArray<SpriteInfo>(0, spriteDescriptor.NumberOfSprites);
        var imageData = buffer.Slice((uint)(sizeof(SpriteInfo) * spriteDescriptor.NumberOfSprites));

        var asset = _assets.SafeAlloc();
        if (asset == null)
        {
            Logger.Error<SpriteLoader>("Failed to allocate a slot in the pool.");
            return null;
        }

        asset->Texture = _resourceManager->CreateTexture(new CreateTextureArgs
        {
            Height = texture.Height,
            Width = texture.Width,
            Format = texture.DXGIFormat,
            InitialData = imageData,
            ShaderVisible = true
        });

        if (asset->Texture.IsInvalid)
        {
            Logger.Error<SpriteLoader>("Failed to create the Texture");
            return null;
        }
        asset->TextureId = _resourceManager->Access(asset->Texture)->SRV.Index;
        asset->Coordinates = SafeAllocArray(sprites.Length);
        if (!asset->Coordinates.IsValid)
        {
            Logger.Error<SpriteLoader>("Failed to allocate memory for texture coordinates.");
            return null;
        }

        var image = new Vector2(texture.Width, texture.Height);
        for (var i = 0; i < sprites.Length; ++i)
        {
            ref readonly var sprite = ref sprites[i];
            asset->Coordinates[i] = new()
            {
                UVMin = new Vector2(sprite.MinX, sprite.MinY + 1) / image,
                UVMax = new Vector2(sprite.MaxX + 1, sprite.MaxY) / image
            };
        }

        return asset;
    }

    private TitanArray<TextureCoordinate> SafeAllocArray(uint length)
    {
        //NOTE(Jens): Replace with some other synchronization later. spinlock?
        lock (_lock)
        {
            return _allocator.AllocArray<TextureCoordinate>(length);
        }
    }

    private void SafeFreeArray(ref TitanArray<TextureCoordinate> array)
    {
        //NOTE(Jens): Replace with some other synchronization later.
        lock (_lock)
        {
            _allocator.FreeArray(ref array);
        }
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        Logger.Warning<SpriteLoader>("Shutdown has not been implemented yet.");
    }

    public void Unload(SpriteAsset* asset)
    {
        throw new NotImplementedException();
    }
}
