using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Components;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS.Archetypes;

[UnmanagedResource]
internal unsafe partial struct ComponentSystem
{
    private ChunkAllocator _chunkAllocator;
    private ArchetypeRegistry _registry;

    private ComponentCommands _commands;

    [System(SystemStage.Init)]
    public static void Init(ComponentSystem* manager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        Logger.Trace<ComponentSystem>($"Init {nameof(ComponentSystem)}. Max Archetypes = {config.MaxArchetypes} Max Chunks = {config.MaxChunks} Pre Allocated Chunks = {config.PreAllocatedChunks}");


        if (!manager->_chunkAllocator.Init(memoryManager, config.MaxChunks, config.PreAllocatedChunks))
        {
            Logger.Error<ComponentSystem>($"Failed to init the {nameof(ChunkAllocator)}. Max Chunks = {config.MaxChunks} Pre-Allocated chunks = {config.PreAllocatedChunks}");
            return;
        }

        if (!manager->_registry.Init(memoryManager, &manager->_chunkAllocator, config.MaxEntities, config.MaxArchetypes))
        {
            Logger.Error<ComponentSystem>($"Failed to init the {nameof(ArchetypeRegistry)}. Max Entities = {config.MaxEntities} Max Archetypes = {config.MaxArchetypes}");
            return;
        }

        if (!manager->_commands.Init(memoryManager, config.MaxCommands, config.MaxCommandComponentSize))
        {
            Logger.Error<ComponentSystem>($"Failed to init the {nameof(ComponentCommands)}. Max Commands = {config.MaxCommands} Max Commands Components buffer = {config.MaxCommandComponentSize} bytes");
            return;
        }
    }

    [System(SystemStage.PostUpdate)]
    public static void ExecuteCommands(ComponentSystem* system, in InputState inputState)
    {
        ref var registry = ref system->_registry;

        if (inputState.IsKeyReleased(KeyCode.K))
        {
            registry.PrintArchetypeStats();
        }
        var commands = system->_commands.GetCommands();
        if (commands.IsEmpty)
        {
            return;
        }


        var timer = Stopwatch.StartNew();
        foreach (ref readonly var command in commands)
        {
            switch (command.Type)
            {
                case EntityCommandType.AddComponent:
                    registry.AddComponent(command.Entity, command.ComponentType, command.Data);
                    break;
                case EntityCommandType.RemoveComponent:
                    registry.RemoveComponent(command.Entity, command.ComponentType);
                    break;
                case EntityCommandType.DestroyEntity:
                    Logger.Warning<ComponentSystem>("Destroy is not implemented.");
                    break;
            }
        }
        timer.Stop();
        Logger.Error<ComponentSystem>($"Count = {commands.Length} Time : {timer.Elapsed.TotalMilliseconds} ms (Ticks: {timer.Elapsed.Ticks})");
        system->_commands.Reset();
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ComponentSystem* manager, IMemoryManager memoryManager)
    {
        //TODO(Jens): implement this
        Logger.Warning<ComponentSystem>("Shutdown has not been implemented yet. memory leaks are imminent.");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(in Entity entity, in T data = default) where T : unmanaged, IComponent
        => _commands.AddComponent(entity, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
        => _commands.RemoveComponent<T>(entity);


    private static int _counter;

    [System]
    [EntityConfig(Not = [typeof(TransformRect)])]
    public static void EntityTestSystem(ReadOnlySpan<Entity> entities, Span<Transform3D> transforms, ComponentSystem* sys)
    {
        if (_counter++ > 4)
        {
            return;
        }

        var componentId1 = Transform3D.Type.Id;
        var signature = Transform3D.Type.Id;

        ref var reg = ref sys->_registry;

        var timer = Stopwatch.StartNew();
        reg.ConstructQuery();
        timer.Stop();
        Logger.Error<ComponentSystem>($"Constructo query. Elapsed = {timer.Elapsed.TotalMicroseconds} micro seconds ({timer.Elapsed.TotalMilliseconds} ms)");
        Archetype* archetype;
        var index = 0u;
        var a = Stopwatch.StartNew();
        while (reg.EnumerateArchetypes(ref index, signature, &archetype))
        {
            //ref readonly var layout = ref archetype->Layout;

            //layout.
            //var chunkIndex = 0u;
            //Chunk* chunk = archetype->;
            //chunk->Header.

            //while (chunk)
            //{

            //}
            //while (archetype->EnumerateChunks(ref chunkIndex, &chunk))
            //{

            //}
            //Logger.Error<ComponentSystem>($"Archetype match: ID = {archetype->Id.Signature}");
        }
        a.Stop();
        Logger.Error<ComponentSystem>($"Time = {a.Elapsed.TotalMicroseconds}");
        for (var i = 0; i < entities.Length; ++i)
        {
            ref readonly var entity = ref entities[i];
            ref var transform = ref transforms[i];
            Logger.Trace<ComponentSystem>($"Entity: {entity.IdNoVersion} Tranform: {transform.Position}");
            transform.Position += Vector3.One * 0.01f;
        }
    }

   
}


public unsafe struct CachedQuery
{
    private Archetype** _archetypes;
    private ushort* _offsets;

    private ushort _count;
    private ushort _componentCount;


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public bool EnumerateData(ref QueryState state, void** data)
    {
        while (state.Index > _count)
        {
            var archetype = _archetypes[state.Index];

            if (state.Chunk == null)
            {
                state.Chunk = archetype->ChunkStart;
            }

            var offsetStart = state.Index * _componentCount;
            for (var i = 0; i < _componentCount; ++i)
            {
                data[i] = state.Chunk->GetDataRow(_offsets[offsetStart + i]);
            }

            state.Chunk = state.Chunk->Header.Next;
            if (state.Chunk == null)
            {
                state.Index++;
            }
            return true;
        }

        return false;
    }
}

public unsafe ref struct QueryState
{
    internal uint Index;
    internal Chunk* Chunk;
}
