using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Upload;
using Titan.Graphics.Rendering;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.Resources;

[Asset(AssetType.Texture)]
[StructLayout(LayoutKind.Explicit)]
internal partial struct TextureAsset
{
    [FieldOffset(0)]
    public D3D12Texture2D D3D12Texture2D;

    //NOTE(Jens): If we ever want to support another rendering API, figure out how to do this. (Sample provided)
    //[FieldOffset(0)]
    //public VulkanTexture2D VulkanTexture2D;
}

[AssetLoader<TextureAsset>]
internal unsafe partial struct D3D12TextureLoader
{
    private PoolAllocator<TextureAsset> _pool;

    //TODO(Jens): Decide if we should extract this to some "ResourceManager" class/struct instead. 

    private D3D12Device* _device;
    private D3D12UploadQueue* _uploadQueue;
    private D3D12Allocator* _allocator;

    public bool Init(in AssetLoaderInitializer init)
    {
        var config = init.ConfigurationManager.GetConfigOrDefault<D3D12Config>();
        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, config.Resources.MaxTextures))
        {
            Logger.Error<D3D12TextureLoader>($"Failed to allocate memory for the texture pool. Count = {config.Resources.MaxTextures}.");
            return false;
        }

        _device = init.GetResourcePointer<D3D12Device>();
        _uploadQueue = init.GetResourcePointer<D3D12UploadQueue>();
        _allocator = init.GetResourcePointer<D3D12Allocator>();

        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_pool);
    }

    public TextureAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        using var _ = new MeasureTime<D3D12TextureLoader>("Loaded texture in {0} ms");
        Debug.Assert(descriptor.Type == AssetType.Texture);
        Debug.Assert(buffer.Size > 0, "The size of the buffer is zero, this was not expected.");

        var texture = _pool.SafeAlloc();
        if (texture == null)
        {
            Logger.Error<D3D12TextureLoader>("Failed to allocate a Texture from the Pool.");
            return null;
        }

        ref readonly var texture2D = ref descriptor.Texture2D;
        ComPtr<ID3D12Resource> resource = _device->CreateTexture(texture2D.Width, texture2D.Height, texture2D.DXGIFormat);
        if (!resource.IsValid)
        {
            Logger.Error<D3D12TextureLoader>("Failed to create the ID3D12Resource.");
            return null;
        }

        var srv = _allocator->Allocate(DescriptorHeapType.ShaderResourceView);
        if (!srv.IsValid)
        {
            Logger.Error<D3D12TextureLoader>("Failed to allocate a SRV handle.");
            return null;
        }

        if (!_uploadQueue->Upload(resource, buffer))
        {
            Logger.Error<D3D12TextureLoader>("Failed to upload the texture.");
            return null;
        }

        texture->D3D12Texture2D = new()
        {
            SRV = srv,
            RTV = default,
            Resource = resource,
            Texture2D =
            {
                Height = texture2D.Height,
                Width = texture2D.Width
            }
        };

        return texture;

    }

    public void Unload(TextureAsset* asset)
    {
        Debug.Assert(asset != null);

        ref var texture = ref asset->D3D12Texture2D;
        
        if (texture.RTV.IsValid)
        {
            //NOTE(Jens): NOt sure we'll ever have a RTV on a texture that has been loaded as an asset.
            _allocator->Free(texture.RTV);
        }
        if (texture.SRV.IsValid)
        {
            _allocator->Free(texture.SRV);
        }
        texture.Resource.Dispose();
        _pool.SafeFree(asset);
    }
}
