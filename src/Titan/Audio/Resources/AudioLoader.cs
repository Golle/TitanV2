using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Rendering.Resources;

namespace Titan.Audio.Resources;

[Asset(AssetType.Audio)]
public partial struct AudioAsset
{
    public TitanBuffer AudioData;
}

[AssetLoader<AudioAsset>]
internal unsafe partial struct AudioLoader
{
    private GeneralAllocator _allocator;
    private PoolAllocator<AudioAsset> _pool;

    private SpinLock _lock;

    public bool Init(in AssetLoaderInitializer init)
    {
        var config = init.ConfigurationManager.GetConfigOrDefault<AudioConfig>();
        var preAllocated = MemoryUtils.MegaBytes(32);
        Debug.Assert(config.MaxAudioBufferBytes > preAllocated, "Max Audio Bytes is less than the PreAllocated bytes.");
        if (!init.MemoryManager.TryCreateGeneralAllocator(out _allocator, config.MaxAudioBufferBytes, preAllocated))
        {
            Logger.Error<AudioLoader>($"Failed to create the Allocator for Audio Buffer. Size = {config.MaxAudioBufferBytes} bytes, Pre Allocated = {preAllocated} bytes");
            return false;
        }

        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, config.MaxLoadedSounds))
        {
            Logger.Error<AudioLoader>($"Failed to create the Allocator for {nameof(AudioAsset)}. Count = {config.MaxLoadedSounds}. Size = {config.MaxLoadedSounds * sizeof(AudioAsset)}");
            return false;
        }

        return true;
    }

    public AudioAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Debug.Assert(descriptor.Type == AssetType.Audio);
        //ref readonly var audio = ref descriptor.Audio; // no use for this atm

        var asset = _pool.SafeAlloc();
        if (asset == null)
        {
            Logger.Error<AudioLoader>("Failed to allocate a resource. Out of resources in the pool.");
            return null;
        }

        asset->AudioData = SafeAlloc(buffer.Size);
        MemoryUtils.Copy(asset->AudioData, buffer, buffer.Size);
        return asset;
    }

    public void Unload(AudioAsset* asset)
    {
        SafeFree(ref asset->AudioData);
        _pool.Free(asset);
    }

    public bool Reload(AudioAsset* asset, in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        Logger.Warning<AudioLoader>("Reload not implemented");
        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        _allocator.Release();
        _allocator = default;
    }

    private TitanBuffer SafeAlloc(uint size)
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        try
        {
            Debug.Assert(gotLock);
            return _allocator.AllocBuffer(size);
        }
        finally
        {
            _lock.Exit();
        }
    }

    private void SafeFree(ref TitanBuffer buffer)
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        try
        {
            Debug.Assert(gotLock);
            _allocator.FreeBuffer(ref buffer);
        }
        finally
        {
            _lock.Exit();
        }
    }
}
