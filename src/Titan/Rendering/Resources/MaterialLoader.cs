using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory.Allocators;
using Titan.Materials;

namespace Titan.Rendering.Resources;

[Asset(AssetType.Material)]
public partial struct MaterialAsset
{
    public Inline16<Handle<MaterialData>> Materials;
    public int MaterialCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Handle<MaterialData> Get(int index)
    {
        Debug.Assert(index >= 0 && index < MaterialCount);
        return Materials[index];
    }

    /// <summary>
    /// implicit operator returns first material
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle<MaterialData>(in MaterialAsset asset)
        => asset.Get(0);
}

[AssetLoader<MaterialAsset>]
internal unsafe partial struct MaterialLoader
{
    private MaterialsSystem* _materialsSystem;
    private PoolAllocator<MaterialAsset> _materials;

    public bool Init(in AssetLoaderInitializer init)
    {
        const int MaxMaterials = 512;
        if (!init.MemoryManager.TryCreatePoolAllocator(out _materials, MaxMaterials))
        {
            Logger.Error<MeshLoader>("Failed to create a resource pool for meshes.");
            return false;
        }
        _materialsSystem = init.GetResourcePointer<MaterialsSystem>();

        return true;
    }

    public MaterialAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Debug.Assert(descriptor.Type == AssetType.Material);

        var material = _materials.SafeAlloc();
        if (material == null)
        {
            Logger.Error<MaterialLoader>("Failed to alloc a material handle");
            return null;
        }

        TitanBinaryReader reader = new(buffer);
        var dependencyCount = 0;
        for (var i = 0; i < descriptor.Material.MaterialCount; ++i)
        {
            ref readonly var diffuse = ref reader.Read<Color>();
            var diffuseMap = Handle<Texture>.Invalid;
            if (reader.ReadByteAsBool())
            {
                //NOTE(Jens): We assume they are stored in the same order as they are written.
                ref readonly var textureAsset = ref dependencies[dependencyCount++].GetAsset<TextureAsset>();
                diffuseMap = textureAsset.Handle;
            }
            material->Materials[i] = _materialsSystem->CreateMaterial(diffuseMap, diffuse);
            if (material->Materials[i].IsInvalid)
            {
                Logger.Error<MaterialLoader>($"Failed to create the material at index {i}.");
                _materials.SafeFree(material);
                return null;
            }
        }
        material->MaterialCount = descriptor.Material.MaterialCount;
        return material;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_materials);
        _materials = default;
    }

    public void Unload(MaterialAsset* asset)
    {
        Logger.Warning<MaterialLoader>("Unload is not implemented");
    }

    public bool Reload(MaterialAsset* asset, in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        TitanBinaryReader reader = new(buffer);
        foreach (var handle in asset->Materials.AsSpan()[..asset->MaterialCount])
        {
            ref readonly var diffuse = ref reader.Read<Color>();
            if (reader.ReadByteAsBool())
            {
                Logger.Warning<MaterialLoader>("Hot reload does not support reloading dependencies. If you've changed the dependency please restart game and build the registry file.");
            }
            Logger.Error($"Diffuse: {diffuse.R} {diffuse.G} {diffuse.B}");
            _materialsSystem->UpdateDiffuseColor(handle, diffuse);
        }
        return true;
    }
}

