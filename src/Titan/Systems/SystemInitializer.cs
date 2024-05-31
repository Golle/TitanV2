using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.ECS;
using Titan.ECS.Archetypes;
using Titan.Events;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;

public unsafe ref struct SystemInitializer
{
    private readonly Span<uint> _mutable;
    private readonly Span<uint> _readOnly;
    private readonly UnmanagedResourceRegistry _unmanagedResources;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly EventSystem _eventSystem;

    internal SystemInitializer(UnmanagedResourceRegistry unmanagedResources, ServiceRegistry serviceRegistry, EventSystem eventSystem, Span<uint> mutable, Span<uint> readOnly)
    {
        _unmanagedResources = unmanagedResources;
        _serviceRegistry = serviceRegistry;
        _eventSystem = eventSystem;
        _mutable = mutable;
        _readOnly = readOnly;
    }

    internal int MutableCount { get; private set; }
    internal int ReadOnlyCount { get; private set; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetMutableResource<T>() where T : unmanaged, IResource
    {
        Debug.Assert(MutableCount < _mutable.Length);
        _mutable[MutableCount++] = T.Id;
        return _unmanagedResources.GetResourcePointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetReadOnlyResource<T>() where T : unmanaged, IResource
    {
        Debug.Assert(ReadOnlyCount < _readOnly.Length);
        _readOnly[ReadOnlyCount++] = T.Id;
        return _unmanagedResources.GetResourcePointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EventReader<T> CreateEventReader<T>() where T : unmanaged, IEvent
        => _eventSystem.CreateReader<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EventWriter CreateEventWriter()
        => _eventSystem.CreateWriter();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ManagedResource<T> GetService<T>() where T : class, IService
        => _serviceRegistry.GetHandle<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EntityManager CreateEntityManager()
        => new(_unmanagedResources.GetResourcePointer<EntitySystem>(), _unmanagedResources.GetResourcePointer<ComponentSystem>());
}
