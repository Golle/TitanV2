using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;

namespace Titan.Rendering.Resources;

[Asset(AssetType.Texture)]
[StructLayout(LayoutKind.Sequential, Size = 8)] // Force the size to 8 , or pool allocator will fail. Rework of Pool allocator planned.
public partial struct TextureAsset
{
    public Handle<Texture> Handle;
}

[AssetLoader<TextureAsset>]
internal unsafe partial struct TextureLoader
{
    private PoolAllocator<TextureAsset> _pool;
    private D3D12ResourceManager* _resourceManager;

    public bool Init(in AssetLoaderInitializer init)
    {
        var config = init.ConfigurationManager.GetConfigOrDefault<D3D12Config>();
        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, config.Resources.MaxTextures))
        {
            Logger.Error<TextureLoader>($"Failed to allocate memory for the texture pool. Count = {config.Resources.MaxTextures}.");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();

        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_pool);
    }

    public TextureAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        using var _ = new MeasureTime<TextureLoader>("Loaded texture in {0} ms");
        Debug.Assert(descriptor.Type == AssetType.Texture);
        Debug.Assert(buffer.Size > 0, "The size of the buffer is zero, this was not expected.");

        ref readonly var texture2D = ref descriptor.Texture2D;

        var asset = _pool.Alloc();
        if (asset == null)
        {
            Logger.Error<TextureLoader>("Failed to allocate a slot for the texture asset.");
            return null;
        }

        asset->Handle = _resourceManager->CreateTexture(new CreateTextureArgs
        {
            Format = texture2D.DXGIFormat,
            Height = texture2D.Height,
            Width = texture2D.Width,
            ShaderVisible = true,
            RenderTargetView = false,
            InitialData = buffer
        });

        if (asset->Handle.IsInvalid)
        {
            Logger.Error<TextureLoader>("Failed to load the texture.");
            return null;
        }

        return asset;
    }

    public void Unload(TextureAsset* asset)
    {
        Debug.Assert(asset != null);
        _resourceManager->DestroyTexture(asset->Handle);
    }
}
