using System.Runtime.CompilerServices;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Input;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS.Archetypes;

[UnmanagedResource]
internal unsafe partial struct ComponentSystem
{
    private ArchetypeRegistry* _registry;
    private ComponentCommands _commands;

    [System(SystemStage.PreInit)]
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

    //TODO(Jens): See if we should move this to last. Making it possible to interact with entities in both PreUpdate, Update and PostUpdate.
    [System(SystemStage.PostUpdate)]
    public static void ExecuteCommands(ComponentSystem* system, in InputState inputState)
    {
        ref var registry = ref system->_registry;

        if (inputState.IsKeyReleased(KeyCode.F1))
        {
            registry->PrintArchetypeStats();
        }

        var commands = system->_commands.GetCommands();
        if (commands.IsEmpty)
        {
            return;
        }

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
        system->_commands.Reset();
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(ComponentSystem* manager, IMemoryManager memoryManager)
    {
        manager->_commands.Shutdown(memoryManager);
        *manager = default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddComponent<T>(in Entity entity, in T data = default) where T : unmanaged, IComponent
        => _commands.AddComponent(entity, data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
        => _commands.RemoveComponent<T>(entity);
}
