using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;

namespace Titan.Graphics.Resources;


[Asset(AssetType.ShaderConfig)]
public unsafe partial struct ShaderInfo
{
    public ShaderAsset* VertexShader;
    public ShaderAsset* PixelShader;
}

[AssetLoader<ShaderInfo>]
public unsafe partial struct ShaderInfoLoader
{
    private PoolAllocator<ShaderInfo> _pool;

    public bool Init(in AssetLoaderInitializer init)
    {
        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, 1024))
        {
            Logger.Error<ShaderInfoLoader>("Failed to create the resource pool for shaders");
            return false;
        }
        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_pool);
    }

    public ShaderInfo* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        var resource = _pool.SafeAlloc();
        if (resource == null)
        {
            Logger.Error<ShaderInfoLoader>($"Failed to alloc a {nameof(ShaderInfo)} from the pool.");
            return null;
        }
        *resource = default;

        foreach (var dependency in dependencies)
        {
            var shader = dependency.GetAssetPointer<ShaderAsset>();
            if (shader->ShaderType == ShaderType.Vertex)
            {
                Debug.Assert(resource->VertexShader == null);
                resource->VertexShader = shader;
            }
            else if (shader->ShaderType == ShaderType.Pixel)
            {
                Debug.Assert(resource->PixelShader == null);
                resource->PixelShader = shader;
            }
            else
            {
                Logger.Warning<ShaderInfoLoader>($"Shader type {shader->ShaderType} is not supported/implemented.");
            }
        }

        //NOTE(Jens): sanity check for now.
        Debug.Assert(resource->PixelShader != null);
        Debug.Assert(resource->VertexShader != null);

        return resource;
    }

    public void Unload(ShaderInfo* asset)
    {
        _pool.SafeFree(asset);
    }
}
