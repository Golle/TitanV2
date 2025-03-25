using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Resources;
using Titan.Systems;

namespace Titan.ECS.Archetypes;

[UnmanagedResource]
internal unsafe partial struct QueryRegistry
{
    private BumpAllocator _allocator;
    private ArchetypeRegistry* _archetypeRegistry;

    private CachedQuery** _queries;
    private uint _queryCount;

    private uint _lastArchetypeCount;

    public bool RegisterQueries(IMemoryManager memoryManager, IReadOnlyList<SystemDescriptor> systems)
    {
        var queries = stackalloc CachedQuery*[systems.Count];
        var count = 0u;
        foreach (var systemDescriptor in systems)
        {
            var query = systemDescriptor.GetQuery();
            if (query != null)
            {
                queries[count++] = query;
            }
        }
        Logger.Trace<QueryRegistry>($"Registered {count} entity queries.");

        if (count > 0)
        {
            _queries = (CachedQuery**)memoryManager.Alloc((uint)(count * sizeof(CachedQuery*)));
            if (_queries == null)
            {
                Logger.Error<QueryRegistry>($"Failed to register the queries. Count = {count}");
                return false;
            }

            MemoryUtils.Copy(_queries, queries, (uint)(count * sizeof(CachedQuery*)));
        }

        _queryCount = count;
        return true;
    }

    [System(SystemStage.PreInit)]
    internal static void Init(QueryRegistry* registry, UnmanagedResourceRegistry unmanagedResources, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<ECSConfig>();

        if (!memoryManager.TryCreateBumpAllocator(out registry->_allocator, config.MaxQuerySize))
        {
            Logger.Error<QueryRegistry>("Failed to create the Query allocator.");
            return;
        }

        registry->_archetypeRegistry = unmanagedResources.GetResourcePointer<ArchetypeRegistry>();
    }

    [System(SystemStage.PreUpdate)]
    internal static void CheckForDirtyQueries(QueryRegistry* registry)
    {
        var count = registry->_archetypeRegistry->ArchetypeCount;
        if (registry->_lastArchetypeCount != count)
        {
            registry->UpdateQueries();
            Logger.Info<QueryRegistry>("Dirty archetypes! Re-build queries.");
        }

        registry->_lastArchetypeCount = registry->_archetypeRegistry->ArchetypeCount;
    }

    private void UpdateQueries()
    {
        var archetypeCount = _archetypeRegistry->ArchetypeCount;
        
        //TODO(Jens): Stack overflow could happen, but leave it as is for now and revisit.
        //maybe we should use a titan buffer for this.
        var archetypeBuffer = stackalloc Archetype*[(int)archetypeCount];
        var offsetBuffer = stackalloc ushort[(int)archetypeCount * 10];

        _allocator.Reset();
        for (var i = 0; i < _queryCount; ++i)
        {
            var query = _queries[i];
            _archetypeRegistry->UpdateQuery(ref _allocator, query, archetypeBuffer, offsetBuffer);
        }
    }

    [System(SystemStage.PostShutdown)]
    internal static void Shutdown(QueryRegistry* registry, IMemoryManager memoryManager)
    {
        memoryManager.FreeAllocator(registry->_allocator);
        if (registry->_queries != null)
        {
            memoryManager.Free(registry->_queries);
        }
    }
}
