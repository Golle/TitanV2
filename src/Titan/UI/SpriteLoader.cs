using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Rendering;

namespace Titan.UI;


[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct SpriteInfo
{
    public ushort X;
    public ushort Y;
    public ushort Width;
    public ushort Height;
}

[Asset(AssetType.Sprite)]
public partial struct SpriteAsset
{
    public Handle<Texture> Texture;
    private uint Padding;
}

[AssetLoader<SpriteAsset>]
internal unsafe partial struct SpriteLoader
{
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
        ref readonly var sprite = ref descriptor.Sprite;
        ref readonly var texture = ref sprite.Texture;

        var sprites = buffer.SliceArray<SpriteInfo>(0, sprite.NumberOfSprites);
        var imageData = buffer.Slice((uint)(sizeof(SpriteInfo) * sprite.NumberOfSprites));

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
            _assets.SafeFree(asset);
            Logger.Error<SpriteLoader>("Failed to create the Texture");
            return null;
        }
        return asset;
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
