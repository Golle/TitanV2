using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics.D3D12;
using Titan.Rendering.Storage;

namespace Titan.Rendering.Resources;

[AssetLoader<MeshAsset>]
internal unsafe partial struct MeshLoader
{
    private PoolAllocator<MeshAsset> _meshes;
    private D3D12ResourceManager* _resourceManager;
    private MeshStorage* _meshStorage;

    public bool Init(in AssetLoaderInitializer init)
    {
        if (!init.MemoryManager.TryCreatePoolAllocator(out _meshes, 1024))
        {
            Logger.Error<MeshLoader>("Failed to create a resource pool for meshes.");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();
        _meshStorage = init.GetResourcePointer<MeshStorage>();
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

        var mesh = _meshes.SafeAlloc();
        if (mesh == null)
        {
            Logger.Error<MeshLoader>("Failed to alloc a mesh from the pool");
            return null;
        }

        mesh->MeshDataHandle = _meshStorage->CreateMesh(new CreateMeshArgs
        {
            SubMeshes = subMeshes,
            Vertices = vertices,
            Indices = indices
        });

        return mesh;
    }

    public void Unload(MeshAsset* asset)
    {
        *asset = default;
        _meshes.SafeFree(asset);
    }
}

[Asset(AssetType.Mesh)]
[StructLayout(LayoutKind.Sequential, Size = 8)]
public partial struct MeshAsset
{
    internal Handle<MeshData> MeshDataHandle;
}

public struct SubMesh
{
    public int VertexOffset;
    public int VertexCount;
    public int IndexOffset;
    public int IndexCount;
}
