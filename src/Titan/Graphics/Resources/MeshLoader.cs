using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;

namespace Titan.Graphics.Resources;

[AssetLoader<MeshAsset>]
internal unsafe partial struct MeshLoader
{
    private PoolAllocator<MeshAsset> _meshes;
    private D3D12ResourceManager* _resourceManager;

    public bool Init(in AssetLoaderInitializer init)
    {
        if (!init.MemoryManager.TryCreatePoolAllocator(out _meshes, 1024))
        {
            Logger.Error<MeshLoader>("Failed to create a resource pool for meshes.");
            return false;
        }

        _resourceManager = init.GetResourcePointer<D3D12ResourceManager>();
        return true;
    }

    public void Shutdown(in AssetLoaderInitializer init)
    {
        init.MemoryManager.FreeAllocator(_meshes);
    }

    public MeshAsset* Load(in AssetDescriptor descriptor, TitanBuffer buffer)
    {
        Debug.Assert(descriptor.Type == AssetType.Mesh);
        ref readonly var meshDescriptor = ref descriptor.Mesh;
        var subMeshes = buffer.SliceArray<SubMesh>(0, meshDescriptor.SubMeshCount);
        var vertices = buffer.SliceArray<Vertex>((uint)sizeof(SubMesh) * meshDescriptor.SubMeshCount, meshDescriptor.VertexCount);

        var mesh = _meshes.SafeAlloc();
        if (mesh == null)
        {
            Logger.Error<MeshLoader>("Failed to alloc a mesh from the pool");
            return null;
        }

        mesh->VertexBuffer = _resourceManager->CreateBuffer(new CreateBufferArgs(vertices.Length, sizeof(Vertex), BufferType.Vertex, vertices.AsBuffer()));
        mesh->SubMeshCount = subMeshes.Length;
        subMeshes.AsReadOnlySpan().CopyTo(mesh->SubMeshes);

        //NOTE(Jens): We Fake an index buffer since it hasn't been implemented yet.
        var indices = stackalloc uint[(int)vertices.Length];
        for (var i = 0u; i < vertices.Length; ++i)
        {
            indices[i] = i;
        }

        //mesh->IndexBuffer = _resourceManager->CreateBuffer(new CreateBufferArgs(vertices.Length, sizeof(ushort), BufferType.Index, new TitanBuffer(indices, sizeof(ushort) * vertices.Length)));
        mesh->IndexBuffer = _resourceManager->CreateBuffer(new CreateBufferArgs(vertices.Length, sizeof(uint), BufferType.Index, new TitanBuffer(indices, sizeof(uint) * vertices.Length)));
        mesh->IndexCount = vertices.Length;
        return mesh;
    }

    public void Unload(MeshAsset* asset)
    {
        _resourceManager->DestroyBuffer(asset->VertexBuffer);
        _resourceManager->DestroyBuffer(asset->IndexBuffer);
        *asset = default;
        _meshes.SafeFree(asset);
    }
}

[Asset(AssetType.Mesh)]
internal partial struct MeshAsset
{
    public Handle<Buffer> VertexBuffer;
    public Handle<Buffer> IndexBuffer;
    public uint SubMeshCount;
    public uint IndexCount;
    public Inline16<SubMesh> SubMeshes;

}

internal struct SubMesh
{
    public int VertexOffset;
    public int VertexCount;
}
