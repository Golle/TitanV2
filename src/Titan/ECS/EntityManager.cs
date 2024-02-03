using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Modules;

namespace Titan.ECS;

internal sealed unsafe class EntityManager : IEntityManager
{
    private IMemorySystem? _memorySystem;
    private TitanArray<Entity> _freeEntities;

    private volatile int _freeListCount;
    private uint _maxEntities;

    public bool Init(IMemorySystem memorySystem, ECSConfig config)
    {
        if (!memorySystem.TryAllocArray(out _freeEntities, config.MaxEntities))
        {
            Logger.Error<EntityManager>($"Failed to allocate memory. Entities = {config.MaxEntities} Size = {sizeof(Entity) * config.MaxEntities}");
            return false;
        }

        _maxEntities = config.MaxEntities;
        _memorySystem = memorySystem;
        _freeListCount = (int)_maxEntities;


        // init the entities, this will be sorted in following order [5, 4, 3, 2, 1].
        // When an Entity is created it will remove it from the back of the list, so it will always start with index 0.
        for (var i = 0u; i < _maxEntities; ++i)
        {
            _freeEntities[i] = new Entity(_maxEntities - i, 1);
        }

        return true;
    }


    public void Shutdown()
    {
        if (_memorySystem != null)
        {
            _memorySystem.FreeArray(ref _freeEntities);
        }
        _memorySystem = null;
    }

    public Entity Create()
    {
        var index = Interlocked.Decrement(ref _freeListCount);
        if (index < 0u)
        {
            return Entity.Invalid;
        }
        return _freeEntities[index];
    }

    public void Destroy(Entity entity)
    {
        Debug.Assert(entity.IsValid);
        // ReSharper disable once NonAtomicCompoundOperator
        var index = _freeListCount++; // copy value first
        _freeEntities[index] = new(entity.Id, unchecked((byte)(entity.Version + 1)));
    }
}
