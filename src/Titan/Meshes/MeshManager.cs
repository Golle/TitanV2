using Titan.Core;
using Titan.Rendering;
using Titan.Rendering.Storage;

namespace Titan.Meshes;

public ref struct CreateMeshArgs2
{
    public Handle<MeshData> Mesh { get; init; }
    public static CreateMeshArgs2 FromMesh(Handle<MeshData> handle) => new() { Mesh = handle };
    public static CreateMeshArgs2 FromVertices(ReadOnlySpan<Vertex> vertice, ReadOnlySpan<ushort> indices) => throw new NotImplementedException("Yep!");
}

public readonly unsafe struct MeshManager
{
    private readonly MeshStorage* _storage;
    
    internal MeshManager(MeshStorage* storage)
    {
        _storage = storage;
    }
}
