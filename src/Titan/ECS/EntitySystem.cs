using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Events;
using Titan.Events;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS;

[UnmanagedResource]
internal unsafe partial struct EntitySystem
{
    private TitanArray<Entity> _freeList;
    private EventWriter _writer;
    private volatile int _freeListCount;

    [System(SystemStage.PreInit)]
    public static void Init(EntitySystem* system, IMemoryManager memoryManager, EventWriter writer, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        Logger.Trace<EntitySystem>($"Init event system. Max Entities = {config.MaxEntities}");
        if (!memoryManager.TryAllocArray(out system->_freeList, config.MaxEntities))
        {
            Logger.Error<EntitySystem>($"Failed to allocate memory. Entities = {config.MaxEntities} Size = {sizeof(Entity) * config.MaxEntities}");
            return;
        }

        system->_freeListCount = (int)config.MaxEntities;
        system->_writer = writer;
        // init the entities, this will be sorted in following order [5, 4, 3, 2, 1].
        // When an Entity is created it will remove it from the back of the list, so it will always start with index 0.
        for (var i = 0u; i < config.MaxEntities; ++i)
        {
            system->_freeList[i] = new Entity(config.MaxEntities - i, 1);
        }
    }


    [System(SystemStage.PostShutdown)]
    public static void Shutdown(EntitySystem* system, IMemoryManager memoryManager)
    {
        if (system->_freeList.IsValid)
        {
            memoryManager.FreeArray(ref system->_freeList);
        }
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(EntitySystem* system, EventReader<EntityDestroyedEvent> entityDestroyed)
    {
        if (entityDestroyed.HasEvents)
        {
            foreach (ref readonly var @event in entityDestroyed)
            {
                //Logger.Info<EntitySystem>($"Entity destroyed :O: Entity = {@event.Entity.Id}");

                var version = unchecked((byte)(@event.Entity.Version + 1));
                var id = @event.Entity.IdNoVersion;
                system->_freeList[system->_freeListCount++] = new(id, version);
            }
        }
    }

    public Entity Create()
    {
        var index = Interlocked.Decrement(ref _freeListCount);
        if (index < 0u)
        {
            //NOTE(Jens): this is not good, since we'll just decrease the counter.
            return Entity.Invalid;
        }
        return _freeList[index];
    }

    public void Destroy(Entity entity)
    {
        //Logger.Trace<EntitySystem>($"Destroying entity: {entity.IdNoVersion}");
        _writer.Send(new EntityDestroyedEvent(entity));
    }
}
