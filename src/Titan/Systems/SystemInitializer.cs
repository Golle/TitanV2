using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Audio;
using Titan.Core;
using Titan.ECS;
using Titan.ECS.Archetypes;
using Titan.ECS.Components;
using Titan.Events;
using Titan.Graphics.D3D12;
using Titan.Input;
using Titan.Materials;
using Titan.Meshes;
using Titan.Resources;
using Titan.Services;
using Titan.UI;
using Titan.UI2;

namespace Titan.Systems;

public unsafe ref struct SystemInitializer
{
    private readonly Span<uint> _mutable;
    private readonly Span<uint> _readOnly;
    private readonly UnmanagedResourceRegistry _unmanagedResources;
    private readonly ServiceRegistry _serviceRegistry;
    private readonly EventSystem _eventSystem;
    private Inline8<uint> _mutableComponentTracker;
    private Inline8<uint> _readonlyComponentTracker;
    private byte _mutableComponentCount;
    private byte _readonlyComponentCount;
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
        => new(
            _unmanagedResources.GetResourcePointer<EntitySystem>(),
            _unmanagedResources.GetResourcePointer<ComponentSystem>()
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AssetsManager CreateAssetsManager()
        => new(_unmanagedResources.GetResourcePointer<AssetSystem>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AudioManager CreateAudioManager()
        => new(_unmanagedResources.GetResourcePointer<AudioSystem>());

    public UIManager CreateUIManager()
        => new(
            _unmanagedResources.GetResourcePointer<UISystem>(),
            _unmanagedResources.GetResourcePointer<UISystem2>(),
            _unmanagedResources.GetResourcePointer<InputState>(),
            CreateAssetsManager()
            );

    public MaterialsManager CreateMaterialsManager()
        => new(
            _unmanagedResources.GetResourcePointer<MaterialsSystem>(),
            _unmanagedResources.GetResourcePointer<D3D12ResourceManager>()
        );

    public MeshManager CreateMeshManager()
        => new(
            _unmanagedResources.GetResourcePointer<MeshSystem>()
        );
    public ReadOnlyStorage<T> CreateReadOnlyStorage<T>() where T : unmanaged, IComponent
    {
        AddReadOnlyComponent(T.Type, T.IsTag);
        return new ReadOnlyStorage<T>(_unmanagedResources.GetResourcePointer<ArchetypeRegistry>());
    }

    public MutableStorage<T> CreateMutableStorage<T>() where T : unmanaged, IComponent
    {
        //TODO(Jens): Should we really track dependencies for these? :)
        AddMutableComponent(T.Type, T.IsTag);
        return new MutableStorage<T>(_unmanagedResources.GetResourcePointer<ArchetypeRegistry>());
    }

    public void AddReadOnlyComponent(in ComponentType type, bool isTag)
    {
        if (isTag)
        {
            //NOTE(Jens): We ignore tags for dependencies.
            return;
        }

        Debug.Assert(ReadOnlyCount < _readOnly.Length);

        // Check for duplicates
        for (var i = 0; i < _readonlyComponentCount; ++i)
        {
            if (_readonlyComponentTracker[i] == type.Id)
            {
                return;
            }
        }

        //NOTE(Jens): We offset the ID with the highest in the UnmanagedResources, so no extra work has to be done to support components.
        var id = _unmanagedResources.HighestId + type.Id;
        _readonlyComponentTracker[_readonlyComponentCount++] = type.Id;
        _readOnly[ReadOnlyCount++] = id;
    }

    public void AddMutableComponent(in ComponentType type, bool isTag)
    {
        // Check for duplicates
        for (var i = 0; i < _mutableComponentCount; ++i)
        {
            if (_mutableComponentTracker[i] == type.Id)
            {
                return;
            }
        }

        Debug.Assert(isTag == false, "Can't have mutable reference to Tag components.");
        Debug.Assert(MutableCount < _mutable.Length);

        //NOTE(Jens): We offset the ID with the highest in the UnmanagedResources, so no extra work has to be done to support components.
        var id = _unmanagedResources.HighestId + type.Id;
        _mutableComponentTracker[_mutableComponentCount++] = type.Id;
        _mutable[MutableCount++] = id;
    }
}
