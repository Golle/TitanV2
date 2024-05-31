using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS.Archetypes;

[UnmanagedResource]
internal unsafe partial struct ComponentSystem
{
    public TitanArray<Archetype> Archetypes;
    public TitanArray<Chunk> Chunks;

    [System(SystemStage.Init)]
    public static void Init(ComponentSystem* manager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        if (!memoryManager.TryAllocArray(out manager->Archetypes, 1024))
        {
            Logger.Error<ComponentSystem>("Failed to allocate memory for the Archetypes.");
            return;
        }

        if (!memoryManager.TryAllocArray(out manager->Chunks, config.PreAllocatedChunks))
        {
            Logger.Error<ComponentSystem>($"Failed to allocate memory for the chunks. Count = {config.PreAllocatedChunks}. Size = {sizeof(Chunk) * config.PreAllocatedChunks} bytes.");
            return;
        }
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ComponentSystem* manager, IMemoryManager memoryManager)
    {
        if (manager->Archetypes.IsValid)
        {
            memoryManager.FreeArray(ref manager->Archetypes);
        }
        if (manager->Chunks.IsValid)
        {
            memoryManager.FreeArray(ref manager->Chunks);
        }
    }


    public void AddComponent<T>(in Entity entity) where T : unmanaged, IComponent
    {
        var componentId = T.ComponentId;
        Logger.Trace<ComponentSystem>($"Adding component {componentId.Value} to entity {entity.IdNoVersion}");
    }

    public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
    {
        var componentId = T.ComponentId;
        Logger.Trace<ComponentSystem>($"Removing component {componentId.Value} from entity {entity.IdNoVersion}");
    }
}

