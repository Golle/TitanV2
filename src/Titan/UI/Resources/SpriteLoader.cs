using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Rendering;

namespace Titan.UI.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct SpriteInfo
{
    public ushort MinX, MinY, MaxX, MaxY;
}

public struct NinePatchSpriteInfo
{
    public byte Left, Top, Right, Bottom;
}

[Asset(AssetType.Sprite)]
public partial struct SpriteAsset
{
    public Handle<Texture> Texture;
    public int TextureId;
    public TitanArray<TextureCoordinate> Coordinates;
    public TitanArray<Size> Sizes;
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

        var asset = _assets.SafeAlloc();
        if (asset == null)
        {
            Logger.Error<SpriteLoader>("Failed to allocate a slot in the pool.");
            return null;
        }

        // 1 slot for sprite, 10 slots for NinePatch
        //TODO(Jens): merge these into a single alloc when we know how it should work.
        var spriteCount = (uint)(spriteDescriptor.NumberOfSprites + spriteDescriptor.NumberOfNinePatchSprites * 9);
        if (spriteCount == 0)
        {
            Logger.Warning<SpriteLoader>("The Sprite that is loaded does not contain any sprite definitions.");
            return asset;
        }
        asset->Coordinates = SafeAllocArray<TextureCoordinate>(spriteCount);
        asset->Sizes = SafeAllocArray<Size>(spriteCount);
        if (!asset->Coordinates.IsValid || !asset->Sizes.IsValid)
        {
            Logger.Error<SpriteLoader>("Failed to allocate memory for texture coordinates.");
            return null;
        }
        var reader = new TitanBinaryReader(buffer);

        var image = new Vector2(texture.Width, texture.Height);
        var coordinateOffset = 0u;
        for (var i = 0u; i < spriteDescriptor.NumberOfSprites; ++i)
        {
            var isNinePatch = reader.Read<bool>();
            ref readonly var sprite = ref reader.Read<SpriteInfo>();
            asset->Sizes[coordinateOffset] = new(sprite.MaxX - sprite.MinX, sprite.MinY - sprite.MaxY);
            asset->Coordinates[coordinateOffset] = new()
            {
                UVMin = new Vector2(sprite.MinX, sprite.MinY) / image,
                UVMax = new Vector2(sprite.MaxX, sprite.MaxY) / image
            };

            coordinateOffset++;

            if (isNinePatch)
            {
                ref readonly var ninePatch = ref reader.Read<NinePatchSpriteInfo>();
                InitNinePatch(
                    asset->Coordinates.Slice(coordinateOffset, 9),
                    asset->Sizes.Slice(coordinateOffset, 9),
                    image,
                    sprite,
                    ninePatch
                );
                coordinateOffset += 9;
            }
        }

        var imageData = buffer.Slice(reader.BytesRead);
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
        return asset;
    }

    private static void InitNinePatch(TitanArray<TextureCoordinate> slice, TitanArray<Size> sizes, in Vector2 imageSize, in SpriteInfo sprite, in NinePatchSpriteInfo ninePatch)
    {
        Debug.Assert(slice.Length == 9);

        var x1 = sprite.MinX;
        var x2 = sprite.MinX + ninePatch.Left;
        var x3 = sprite.MaxX - ninePatch.Right;
        var x4 = sprite.MaxX;

        var y1 = sprite.MinY;
        var y2 = sprite.MinY - ninePatch.Bottom;
        var y3 = sprite.MaxY + ninePatch.Top;
        var y4 = sprite.MaxY;

        slice[0] = new(new(x1, y1), new(x2, y2));
        slice[1] = new(new(x2, y1), new(x3, y2));
        slice[2] = new(new(x3, y1), new(x4, y2));

        slice[3] = new(new(x1, y2), new(x2, y3));
        slice[4] = new(new(x2, y2), new(x3, y3));
        slice[5] = new(new(x3, y2), new(x4, y3));

        slice[6] = new(new(x1, y3), new(x2, y4));
        slice[7] = new(new(x2, y3), new(x3, y4));
        slice[8] = new(new(x3, y3), new(x4, y4));

        sizes[0] = new(x2 - x1, y1 - y2);
        sizes[1] = new(x3 - x2, y1 - y2);
        sizes[2] = new(x4 - x3, y1 - y2);

        sizes[3] = new(x2 - x1, y2 - y3);
        sizes[4] = new(x3 - x2, y2 - y3);
        sizes[5] = new(x4 - x3, y2 - y3);

        sizes[6] = new(x2 - x1, y3 - y4);
        sizes[7] = new(x3 - x2, y3 - y4);
        sizes[8] = new(x4 - x3, y3 - y4);

        var vector = (Vector2*)slice.AsPointer();
        for (var i = 0; i < 18; ++i)
        {
            vector[i] /= imageSize;
        }
    }

    private TitanArray<T> SafeAllocArray<T>(uint length) where T : unmanaged
    {
        //NOTE(Jens): Replace with some other synchronization later. spinlock?
        lock (_lock)
        {
            return _allocator.AllocArray<T>(length);
        }
    }

    private void SafeFreeArray<T>(ref TitanArray<T> array) where T : unmanaged
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

    public bool Reload(SpriteAsset* asset, in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        Logger.Trace<SpriteLoader>("Reloading asset.");

        var spriteCount = descriptor.Sprite.NumberOfSprites;
        var ninePatchScount = descriptor.Sprite.NumberOfNinePatchSprites;
        var totalSize =
            spriteCount * sizeof(SpriteInfo) +
            ninePatchScount * sizeof(NinePatchSpriteInfo) +
            spriteCount * sizeof(byte);

        var imageData = buffer.Slice(totalSize);
        return _resourceManager->Upload(asset->Texture, imageData);
    }
}
