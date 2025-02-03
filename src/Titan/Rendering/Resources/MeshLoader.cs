using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Materials;
using Titan.Meshes;

namespace Titan.Rendering.Resources;

[AssetLoader<MeshAsset>]
internal unsafe partial struct MeshLoader
{
    private PoolAllocator<MeshAsset> _meshes;
    private MeshSystem* _meshSystem;

    public bool Init(in AssetLoaderInitializer init)
    {
        if (!init.MemoryManager.TryCreatePoolAllocator(out _meshes, 1024))
        {
            Logger.Error<MeshLoader>("Failed to create a resource pool for meshes.");
            return false;
        }

        _meshSystem = init.GetResourcePointer<MeshSystem>();
        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_meshes);
    }

    public MeshAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
    {
        Debug.Assert(descriptor.Type == AssetType.Mesh);
        ref readonly var meshDescriptor = ref descriptor.Mesh;

        var verticesOffset = meshDescriptor.SubMeshCount * sizeof(SubMesh);
        var indicesOffset = verticesOffset + meshDescriptor.VertexCount * sizeof(Vertex);

        var subMeshes = buffer.SliceArray<SubMesh>(0, meshDescriptor.SubMeshCount);
        var vertices = buffer.SliceArray<Vertex>((uint)verticesOffset, meshDescriptor.VertexCount);
        var indices = buffer.SliceArray<uint>((uint)indicesOffset, meshDescriptor.IndexCount);

        var materials = dependencies.Length > 0
            ? dependencies[0].GetAsset<MaterialAsset>().GetMaterials()
            : ReadOnlySpan<Handle<MaterialData>>.Empty;

        var mesh = _meshes.SafeAlloc();
        if (mesh == null)
        {
            Logger.Error<MeshLoader>("Failed to alloc a mesh from the pool");
            return null;
        }

        mesh->MeshDataHandle = _meshSystem->CreateMesh(new MeshArgs
        {
            Indicies = indices,
            Vertices = vertices,
            SubMeshes = subMeshes,
            Materials = materials
        });

        return mesh;
    }

    public void Unload(MeshAsset* asset)
    {
        *asset = default;
        _meshes.SafeFree(asset);
    }

    public bool Reload(MeshAsset* asset, in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        Logger.Warning<MeshLoader>("Reload not implemented");
        return true;
    }
}

[Asset(AssetType.Mesh)]
[StructLayout(LayoutKind.Sequential, Size = 8)]
public partial struct MeshAsset
{
    internal Handle<MeshData> MeshDataHandle;
    public static implicit operator Handle<MeshData>(in MeshAsset asset) => asset.MeshDataHandle;
}

public struct SubMesh
{
    public int VertexOffset;
    public int VertexCount;
    public int IndexOffset;
    public int IndexCount;
    public int MaterialIndex;
}
