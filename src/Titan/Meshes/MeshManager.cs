using Titan.Core;

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
}
