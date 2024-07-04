using Titan.Assets;
using Titan.ECS.Components;
using Titan.Systems;

namespace Titan.ECS.Systems;
internal partial struct MeshLoaderSystem
{
    [System]
    
    public static void LoadMesh(ReadOnlySpan<Entity> entities, ReadOnlySpan<Mesh3D> meshes, IAssetsManager assetsManager, EntityManager entityManager)
    {
        //TODO(Jens): Do the magic here. 
        // add a check for NOT on the component that has the mesh. The reason to have this system is to create a component in the archetype that can be copied into GPU memory easily.
    }
}
