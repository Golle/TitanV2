using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS.Archetypes;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
[SkipLocalsInit]
internal struct EntityCommand
{
    public unsafe void* Data;
    public ComponentType ComponentType;
    public Entity Entity;
    public EntityCommandType Type;
}

internal enum EntityCommandType : byte
{
    DestroyEntity,
    AddComponent,
    RemoveComponent
}
internal unsafe struct ComponentCommands
{
    private AtomicBumpAllocator _allocator;
    private TitanArray<EntityCommand> _commands;
    private volatile int _commandCount;

    public bool Init(IMemoryManager memoryManager, uint maxEntities, uint maxComponentsSize)
    {
        if (!memoryManager.TryAllocArray(out _commands, maxEntities))
        {
            Logger.Error<ComponentCommands>($"Failed to create the commands array. Max Entites = {maxEntities} Size = {maxEntities * sizeof(EntityCommand)} bytes");
            return false;
        }

        if (!memoryManager.TryCreateAtomicBumpAllocator(out _allocator, maxComponentsSize))
        {
            Logger.Error<ComponentCommands>($"Failed to create the bump allocator. Size = {maxComponentsSize} bytes");
            return false;
        }

        return true;
    }

    public void AddComponent<T>(in Entity entity, in T data) where T : unmanaged, IComponent
    {
        var ptr = (T*)_allocator.Alloc((uint)sizeof(T));
        *ptr = data;

        var index = Interlocked.Increment(ref _commandCount) - 1;
        _commands[index] = new EntityCommand
        {
            Data = ptr,
            Entity = entity,
            Type = EntityCommandType.AddComponent,
            ComponentType = T.Type
        };
    }

    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
    {
        var index = Interlocked.Increment(ref _commandCount) - 1;
        _commands[index] = new EntityCommand
        {
            Entity = entity,
            Type = EntityCommandType.RemoveComponent,
            ComponentType = T.Type
        };
    }

    public void DestroyEntity(in Entity entity)
    {
        var index = Interlocked.Increment(ref _commandCount);
        _commands[index] = new EntityCommand
        {
            Entity = entity,
            Type = EntityCommandType.DestroyEntity
        };
    }

    public ReadOnlySpan<EntityCommand> GetCommands() => _commands.AsReadOnlySpan()[.._commandCount];

    public void Reset()
    {
        _commandCount = 0;
        _allocator.Reset();
    }
}

[UnmanagedResource]
internal unsafe partial struct ComponentSystem
{
    public ChunkAllocator ChunkAllocator;
    public ArchetypeRegistry Registry;

    public ComponentCommands Commands;

    [System(SystemStage.Init)]
    public static void Init(ComponentSystem* manager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        Logger.Trace<ComponentSystem>($"Init {nameof(ComponentSystem)}. Max Archetypes = {config.MaxArchetypes} Max Chunks = {config.MaxChunks} Pre Allocated Chunks = {config.PreAllocatedChunks}");


        if (!manager->ChunkAllocator.Init(memoryManager, config.MaxChunks, config.PreAllocatedChunks))
        {
            Logger.Error<ComponentSystem>("Trololol");
            return;
        }

        if (!manager->Registry.Init(memoryManager, &manager->ChunkAllocator, config.MaxEntities, config.MaxArchetypes))
        {
            Logger.Error<ComponentSystem>("Trololol again");
            return;
        }

        if (!manager->Commands.Init(memoryManager, config.MaxCommands, config.MaxCommandComponentSize))
        {
            Logger.Error<ComponentSystem>($"Failed to init the {nameof(ComponentCommands)}");
            return;
        }
    }

    [System(SystemStage.PostUpdate)]
    public static void ExecuteCommands(ComponentSystem* system, in InputState inputState)
    {
        ref var registry = ref system->Registry;

        if (inputState.IsKeyReleased(KeyCode.K))
        {
            registry.PrintArchetypeStats();
        }
        var commands = system->Commands.GetCommands();
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
        system->Commands.Reset();
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ComponentSystem* manager, IMemoryManager memoryManager)
    {
        //TODO(Jens): implement this
        Logger.Warning<ComponentSystem>("Shutdown has not been implemented yet. memory leaks are imminent.");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(in Entity entity, in T data = default) where T : unmanaged, IComponent
        => Commands.AddComponent(entity, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
        => Commands.RemoveComponent<T>(entity);
}

