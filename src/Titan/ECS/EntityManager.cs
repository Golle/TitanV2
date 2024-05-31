using Titan.ECS.Archetypes;

namespace Titan.ECS;

public readonly unsafe struct EntityManager
{
    private readonly ComponentSystem* _componentSystem;
    private readonly EntitySystem* _entitySystem;

    internal EntityManager(EntitySystem* entitySystem, ComponentSystem* componentSystem)
    {
        _componentSystem = componentSystem;
        _entitySystem = entitySystem;
    }

    public Entity CreateEntity() => _entitySystem->Create();
    public void DestroyEntity(in Entity entity) => _entitySystem->Destroy(entity);
    public void AddComponent<T>(in Entity entity) where T : unmanaged, IComponent => _componentSystem->AddComponent<T>(entity);
    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent => _componentSystem->RemoveComponent<T>(entity);
}

