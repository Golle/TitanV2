using Titan.Core;
using Titan.Rendering;

namespace Titan.Meshes;

public readonly unsafe struct MeshManager
{
    private readonly MeshSystem* _system;

    internal MeshManager(MeshSystem* system)
    {
        _system = system;
    }

    public Handle<MeshData> CreateMesh(in MeshArgs args)
        => _system->CreateMesh(args);


    public Handle<GPUBuffer> GetGPUVertexBufferHandle() => _system->GetVertexBufferHandle();
    public Handle<GPUBuffer> GetGPUIndexBufferHandle() => _system->GetIndexBufferHandle();

    public ref MeshData GetMesh(Handle<MeshData> handle) => ref *_system->Access(handle);
    public MeshData* GetMeshPointer(Handle<MeshData> handle) => _system->Access(handle);

}
