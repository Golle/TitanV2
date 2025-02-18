using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Windows;

namespace Titan.Rendering.Resources;


[Asset(AssetType.Shader)]
public partial struct ShaderAsset
{
    public TitanBuffer ShaderByteCode;
    public ShaderType ShaderType;
}

[AssetLoader<ShaderAsset>]
internal unsafe partial struct ShaderLoader
{
    private PoolAllocator<ShaderAsset> _pool;
    private ManagedResource<IMemoryManager> _memoryManager;
    public bool Init(in AssetLoaderInitializer init)
    {
        var config = init.ConfigurationManager.GetConfigOrDefault<D3D12Config>();

        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, config.Resources.MaxShaders))
        {
            Logger.Error<ShaderLoader>($"Failed to create the {nameof(PoolAllocator<ShaderAsset>)}. Count = {config.Resources.MaxShaders}. Size = {sizeof(ShaderAsset)}");
            return false;
        }

        _memoryManager = init.GetServiceHandle<IMemoryManager>();
        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        Logger.Trace<ShaderLoader>("Shutdown the loader");
        init.MemoryManager.FreeAllocator(_pool);
    }

    public ShaderAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Logger.Trace<ShaderLoader>($"Loading shader of type {descriptor.Shader.Type}. Size = {buffer.Size}");
        var asset = _pool.SafeAlloc();
        if (asset == null)
        {
            Logger.Error<ShaderLoader>("Failed to allocate a resource. Out of resources in the pool.");
            return null;
        }

        asset->ShaderType = descriptor.Shader.Type;
        if (!_memoryManager.Value.TryAllocBuffer(out asset->ShaderByteCode, buffer.Size))
        {
            Logger.Error<ShaderLoader>("Failed to allocate memory for the Shader.");
            _pool.SafeFree(asset);
            return null;
        }

        MemoryUtils.Copy(asset->ShaderByteCode, buffer, buffer.Size);

        return asset;
    }

    public void Unload(ShaderAsset* asset)
    {
        Logger.Trace<ShaderLoader>("Unloading asset.");
        Debug.Assert(asset != null);
        if (asset->ShaderByteCode.IsValid)
        {
            _memoryManager.Value.FreeBuffer(ref asset->ShaderByteCode);
        }
        _pool.SafeFree(asset);
    }

    public bool Reload(ShaderAsset* asset, in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        //NOTE(Jens): We alloc before free so we get a new pointer, if we get the same pointer with same size the reload will not work.
        var oldAlloc = asset->ShaderByteCode;
        _memoryManager.Get().TryAllocBuffer(out asset->ShaderByteCode, buffer.Size);
        _memoryManager.Get().FreeBuffer(ref oldAlloc);
        MemoryUtils.Copy(asset->ShaderByteCode, buffer, buffer.Size);

        return true;
    }
}
