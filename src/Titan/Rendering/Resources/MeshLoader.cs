using System.Diagnostics;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;
using Titan.Graphics;
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
        var subMeshes = buffer.SliceArray<SubMesh>(0, meshDescriptor.SubMeshCount);
        var vertices = buffer.SliceArray<Vertex>((uint)sizeof(SubMesh) * meshDescriptor.SubMeshCount, meshDescriptor.VertexCount);

        //NOTE(Jens): We Fake an index buffer since it hasn't been implemented yet.
        var indices = stackalloc int[(int)vertices.Length];
        for (var i = 0; i < vertices.Length; ++i)
        {
            indices[i] = i;
        }

        var handle = _meshStorage->CreateMesh(new CreateMeshArgs<int>
        {
            Vertices = vertices,
            Indices = new (indices, (int)vertices.Length),
            SubMeshes = subMeshes
        });

        var mesh = _meshes.SafeAlloc();
        if (mesh == null)
        {
            Logger.Error<MeshLoader>("Failed to alloc a mesh from the pool");
            return null;
        }

        mesh->VertexBuffer = _resourceManager->CreateBuffer(new CreateBufferArgs(vertices.Length, sizeof(Vertex), BufferType.Vertex, vertices.AsBuffer()));
        mesh->SubMeshCount = subMeshes.Length;
        subMeshes.AsReadOnlySpan().CopyTo(mesh->SubMeshes);



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

public struct SubMesh
{
    public int VertexOffset;
    public int VertexCount;
}
