using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Configurations;
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
    private ArchetypeRegistry* _registry;
    private ComponentCommands _commands;

    [System(SystemStage.Init)]
    public static void Init(ComponentSystem* manager, in UnmanagedResourceRegistry unmanagedResources, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        Logger.Trace<ComponentSystem>($"Init {nameof(ComponentSystem)}. Max Archetypes = {config.MaxArchetypes} Max Chunks = {config.MaxChunks} Pre Allocated Chunks = {config.PreAllocatedChunks}");

        if (!manager->_commands.Init(memoryManager, config.MaxCommands, config.MaxCommandComponentSize))
        {
            Logger.Error<ComponentSystem>($"Failed to init the {nameof(ComponentCommands)}. Max Commands = {config.MaxCommands} Max Commands Components buffer = {config.MaxCommandComponentSize} bytes");
            return;
        }

        manager->_registry = unmanagedResources.GetResourcePointer<ArchetypeRegistry>();
    }

    [System(SystemStage.PostUpdate)]
    public static void ExecuteCommands(ComponentSystem* system, in InputState inputState)
    {
        ref var registry = ref system->_registry;

        if (inputState.IsKeyReleased(KeyCode.K))
        {
            registry->PrintArchetypeStats();
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
                    registry->AddComponent(command.Entity, command.ComponentType, command.Data);
                    break;
                case EntityCommandType.RemoveComponent:
                    registry->RemoveComponent(command.Entity, command.ComponentType);
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

    //[System]
    //[EntityConfig(Not = [typeof(TransformRect)])]
    public static void EntityTestSystem(ReadOnlySpan<Entity> entities, Span<Transform3D> t1, ComponentSystem* sys, IMemoryManager memoryManager)
    {
        if (_counter++ > 4)
        {
            return;
        }

        var componentId1 = Transform3D.Type.Id;
        var signature = Transform3D.Type.Id;

        ref var reg = ref sys->_registry;

        var timer = Stopwatch.StartNew();

        var archetypeBuffer = stackalloc Archetype*[(int)reg->ArchetypeCount];
        var offsetBuffer = stackalloc ushort[(int)reg->ArchetypeCount * 4];

        var query = reg->ConstructQuery(memoryManager, archetypeBuffer, offsetBuffer, signature);

        timer.Stop();
        Logger.Error<ComponentSystem>($"Constructo query. Elapsed = {timer.Elapsed.TotalMicroseconds} micro seconds ({timer.Elapsed.TotalMilliseconds} ms)");
        Archetype* archetype;
        var index = 0u;


        QueryState state = default;
        var data = stackalloc void*[2];

        var totalCount = 0L;
        var numEntities = 0;
        var componentsRead = 0;
        Logger.Info<ComponentSystem>("Do the query");
        var timer1 = Stopwatch.StartNew();
        Entity* ent;
        while (query.EnumerateData(ref state, &ent, data))
        {
            var count = state.Count;
            numEntities += count;
            var transforms = new Span<Transform3D>(data[0], count);
            var rects = new Span<TransformRect>(data[1], count);

            totalCount += transforms.Length;
            totalCount += rects.Length;

            for (var i = 0; i < count; ++i)
            {
                ref readonly var transform = ref transforms[i];
                ref readonly var rect = ref rects[i];
                totalCount -= (int)transform.Position.X;
                totalCount += rect.Position.X;

                componentsRead += 2;
            }

            //Logger.Info<ComponentSystem>($"Transform3D: {transforms.Length}");
            //Logger.Info<ComponentSystem>($"TransformRect: {rects.Length}");
        }
        timer1.Stop();
        Logger.Info<ComponentSystem>($"Query completed. Number of Entities queried = {numEntities} Components Read = {componentsRead} Elapsed = {timer1.Elapsed.TotalMicroseconds} microseconds ({timer1.Elapsed.TotalMilliseconds} ms) - {totalCount}");


        var q = new CachedQuery([Transform3D.Type], 0);

        while (reg->EnumerateArchetypes(ref index, signature, &archetype))
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


        //query.Free(memoryManager);
        //for (var i = 0; i < entities.Length; ++i)
        //{
        //    ref readonly var entity = ref entities[i];
        //    ref var transform = ref transforms[i];
        //    Logger.Trace<ComponentSystem>($"Entity: {entity.IdNoVersion} Tranform: {transform.Position}");
        //    transform.Position += Vector3.One * 0.01f;
        //}
    }


}

public unsafe ref struct QueryState
{
    public int Count;
    internal uint Index;
    internal Chunk* Chunk;

}
