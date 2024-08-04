using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;

namespace Titan.Rendering.Resources;

[Asset(AssetType.ShaderInfo)]
public unsafe partial struct ShaderInfo
{
    public ShaderAsset* VertexShader;
    public ShaderAsset* PixelShader;
    public Handle<RootSignature> RootSignature;
}

[AssetLoader<ShaderInfo>]
public unsafe partial struct ShaderInfoLoader
{
    private PoolAllocator<ShaderInfo> _pool;
    private D3D12ResourceManager* _resourceManager;

    public bool Init(in AssetLoaderInitializer init)
    {
        if (!init.MemoryManager.TryCreatePoolAllocator(out _pool, 1024))
        {
            Logger.Error<ShaderInfoLoader>("Failed to create the resource pool for shaders");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();

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

        ref readonly var shaderInfo = ref descriptor.ShaderInfo;

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

        var samplers = (SamplerInfo*)buffer.AsPointer();
        var ranges = (DescriptorRangesInfo*)(samplers + shaderInfo.NumberOfSamplers);
        var constantBuffers = (ConstantBufferInfo*)(ranges + shaderInfo.NumberOfConstantBuffers);
        var constants = (ConstantsInfo*)(constantBuffers + shaderInfo.NumberOfConstants);

        //TODO(Jens): See if we want to use some cache for these at some point.
        resource->RootSignature = _resourceManager->CreateRootSignature(new CreateRootSignatureArgs
        {
            ConstantBuffers = new(constantBuffers, shaderInfo.NumberOfConstantBuffers),
            Constants = new(constants, shaderInfo.NumberOfConstants),
            Samplers = new(samplers, shaderInfo.NumberOfSamplers),
            Ranges = new(samplers, shaderInfo.NumberOfSamplers)
        });

        //NOTE(Jens): sanity check for now. Remove when everything is working! :)
        Debug.Assert(resource->PixelShader != null);
        Debug.Assert(resource->VertexShader != null);
        Debug.Assert(resource->RootSignature.IsValid);

        return resource;
    }

    public void Unload(ShaderInfo* asset)
    {
        _resourceManager->DestroyRootSignature(asset->RootSignature);
        _pool.SafeFree(asset);
    }
}