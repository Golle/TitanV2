using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;

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

    public bool Init(IMemoryManager memoryManager, uint maxCommands, uint maxComponentsSize)
    {
        if (!memoryManager.TryAllocArray(out _commands, maxCommands))
        {
            Logger.Error<ComponentCommands>($"Failed to create the commands array. Max Entites = {maxCommands} Size = {maxCommands * sizeof(EntityCommand)} bytes");
            return false;
        }

        if (!memoryManager.TryCreateAtomicBumpAllocator(out _allocator, maxComponentsSize))
        {
            Logger.Error<ComponentCommands>($"Failed to create the bump allocator. Size = {maxComponentsSize} bytes");
            return false;
        }

        return true;
    }

    public void Shutdown(IMemoryManager memoryManager)
    {
        if (_commands.IsValid)
        {
            memoryManager.FreeAllocator(_allocator);
            memoryManager.FreeArray(ref _commands);
        }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<EntityCommand> GetCommands() => _commands.AsReadOnlySpan()[.._commandCount];

    public void Reset()
    {
        _commandCount = 0;
        _allocator.Reset();
    }
}
