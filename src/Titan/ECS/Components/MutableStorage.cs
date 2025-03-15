using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.ECS.Archetypes;

namespace Titan.ECS.Components;

/// <summary>
/// An interface to query components based on the Entity ID.
/// This is slow so only use this for specific actions.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly unsafe struct MutableStorage<T> where T : unmanaged, IComponent
{
    private readonly ArchetypeRegistry* _registry;
    private readonly ComponentType _componentType = T.Type;

    internal MutableStorage(ArchetypeRegistry* registry)
    {
        _registry = registry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent(Entity entity)
        => _registry->HasComponent(entity, _componentType);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(Entity entity)
    {
        var component = _registry->GetComponent(entity, _componentType);
        Debug.Assert(component != null);
        return ref *(T*)component;
    }
}
