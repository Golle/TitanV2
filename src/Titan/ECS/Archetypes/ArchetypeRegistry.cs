using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Components;

namespace Titan.ECS.Archetypes;

internal unsafe struct ArchetypeRegistry
{
    private TitanArray<EntityData> _data;
    private TitanArray<Archetype> _archetypes;
    private uint _archetypeCount;
    private ChunkAllocator* _allocator;

    public bool Init(IMemoryManager memoryManager, ChunkAllocator* allocator, uint maxEntities, uint maxArchetypes)
    {
        if (!memoryManager.TryAllocArray(out _archetypes, maxArchetypes))
        {
            Logger.Error<ArchetypeRegistry>($"Failed to allocate memory for the Archetypes. Count = {maxArchetypes} Size = {maxArchetypes * sizeof(Archetype)} bytes");
            return false;
        }

        if (!memoryManager.TryAllocArray(out _data, maxEntities))
        {
            Logger.Error<ArchetypeRegistry>($"Failed to allocate memory for the {nameof(EntityData)}. Count = {maxEntities} Size = {maxEntities * sizeof(EntityData)} bytes");
            return false;
        }

        // Ensure these are empty
        MemoryUtils.InitArray(_data);
        MemoryUtils.InitArray(_archetypes);

        _allocator = allocator;
        return true;
    }

    public void Shutdown(IMemoryManager memoryManager)
    {
        if (_data.IsValid)
        {
            memoryManager.FreeArray(ref _data);
        }
    }

    public void AddComponent(in Entity entity, in ComponentType type, void* componentData)
    {
        var entityId = entity.IdNoVersion;
        ref var data = ref _data[entityId];
        if (data.IsValid)
        {
            ref var oldRecord = ref data.Record;
            // we use the signature to do a lookup, only create ArchetypeId when we create a new type.
            var signature = oldRecord.Archetype->Id.Signature * type.Id;

            var newArchetype = FindArchetype(signature);
            if (newArchetype == null)
            {
                var archetypeId = oldRecord.Archetype->Id.Add(type);
                newArchetype = CreateArchetype(archetypeId);
            }

            var newRecord = newArchetype->Add(entity);

            // Do the move
            MoveEntityWithAdd(oldRecord, newRecord, type, componentData);
            FreeEntity(oldRecord);

            oldRecord = newRecord;
        }
        else
        {
            // first component
            var archetype = FindArchetype(type);
            if (archetype == null)
            {
                archetype = CreateArchetype(new ArchetypeId(type));
            }
            data.Record = archetype->Add(entity);
            var offset = archetype->Layout.Offsets[0];
            var size = archetype->Layout.Sizes[0];
            var index = data.Record.Index;
            var dataPointer = data.Record.Chunk->GetComponentData(offset, size, index);

            MemoryUtils.Copy(dataPointer, componentData, size);
        }
    }

    public void RemoveComponent(in Entity entity, in ComponentType type)
    {
        var id = entity.IdNoVersion;
        ref var data = ref _data[id];
        ref var source = ref data.Record;

        var archetype = source.Archetype;
        Debug.Assert(archetype != null, "No archetype");

        var newSignature = archetype->Id.Signature / type.Id;
        if (newSignature == 1)
        {
            // last component, delete the record.
            FreeEntity(source);
            data.Record = default;
        }
        else
        {
            var newArchentype = FindArchetype(newSignature);
            if (newArchentype == null)
            {
                var archetypeId = archetype->Id.Remove(type);
                newArchentype = CreateArchetype(archetypeId);
            }

            var destination = newArchentype->Add(entity);
            MoveEntityWithRemove(source, destination, type);
            FreeEntity(source);
            source = destination;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void MoveEntityWithAdd(in ArchetypeRecord source, in ArchetypeRecord destination, in ComponentType type, void* data)
    {
        ref readonly var sourceLayout = ref source.Archetype->Layout;
        ref readonly var destinationLayout = ref destination.Archetype->Layout;
        var count = destinationLayout.NumberOfComponents;

        var fromIndex = 0;
        for (var toIndex = 0; toIndex < count; ++toIndex)
        {
            var toOffset = destinationLayout.Offsets[toIndex];
            var size = destinationLayout.Sizes[toIndex];

            var destionationData = destination.Chunk->GetComponentData(toOffset, size, destination.Index);
            if (destinationLayout.Ids[toIndex] == type.Id)
            {
                MemoryUtils.Copy(destionationData, data, size);
                // this is the new one
                continue;
            }

            Debug.Assert(sourceLayout.Ids[fromIndex] == destinationLayout.Ids[toIndex], "Mismatch in order of component IDs.");
            Debug.Assert(sourceLayout.Sizes[fromIndex] == destinationLayout.Sizes[toIndex], "Mismatch in Size of components");

            var fromOffset = sourceLayout.Offsets[fromIndex];

            var sourceData = source.Chunk->GetComponentData(fromOffset, size, source.Index);
            MemoryUtils.Copy(destionationData, sourceData, size);
            fromIndex++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void MoveEntityWithRemove(in ArchetypeRecord source, in ArchetypeRecord destination, in ComponentType type)
    {
        ref readonly var sourceLayout = ref source.Archetype->Layout;
        ref readonly var destinationLayout = ref destination.Archetype->Layout;
        var count = sourceLayout.NumberOfComponents;

        var destinationIndex = 0;
        for (var sourceIndex = 0; sourceIndex < count; ++sourceIndex)
        {
            if (sourceLayout.Ids[sourceIndex] == type.Id)
            {
                // the component that we're removing.
                continue;
            }

            Debug.Assert(sourceLayout.Ids[sourceIndex] == destinationLayout.Ids[destinationIndex], "Mismatch in order of component IDs.");
            Debug.Assert(sourceLayout.Sizes[sourceIndex] == destinationLayout.Sizes[destinationIndex], "Mismatch in Size of components");

            var sourceOffset = sourceLayout.Offsets[sourceIndex];
            var destinationOffset = destinationLayout.Offsets[destinationIndex];
            var size = sourceLayout.Sizes[sourceIndex];

            var sourceData = source.Chunk->GetComponentData(sourceOffset, size, source.Index);
            var destinationData = destination.Chunk->GetComponentData(destinationOffset, size, destination.Index);

            MemoryUtils.Copy(destinationData, sourceData, size);
            destinationIndex++;
        }
    }

    private void FreeEntity(in ArchetypeRecord oldRecord)
    {
        var archetype = oldRecord.Archetype;
        var result = archetype->Remove(oldRecord);

        //NOTE(Jens): When we release an entity another entity might be changed. If that happens update the metadata
        if (result.Entity.IsValid)
        {
            var data = _data.GetPointer(result.Entity.IdNoVersion);
            data->Record = result.Record;
        }
    }

    private Archetype* CreateArchetype(in ArchetypeId id)
    {
        var archetype = _archetypes.GetPointer(_archetypeCount++);
        *archetype = new Archetype(id, _allocator);
        return archetype;
    }

    private Archetype* FindArchetype(ulong signature)
    {
        //NOTE(Jens): This is where we want to implement some map or tree, to support faster lookups. but if the archetype count is low it might not be needed.
        //NOTE(Jens): the breakoff is around ~10 archetypes,which I think we'll hit early.

        for (var i = 0; i < _archetypeCount; i++)
        {
            if (_archetypes[i].Id.Signature == signature)
            {
                return _archetypes.GetPointer(i);
            }
        }
        return null;
    }

    public void PrintArchetypeStats()
    {
        Logger.Info<ArchetypeRegistry>($"Print archetype stats. Count = {_archetypeCount}");
        foreach (ref readonly var arch in _archetypes.Slice(0, _archetypeCount).AsReadOnlySpan())
        {
            Logger.Info<ArchetypeRegistry>($"Id/Signature: {arch.Id.Signature} Component Size: {arch.Id.ComponentsSize} Number of Entities: {arch.EntitiesCount} Number of Chunks : {arch.ChunkCount}");
        }
    }


    public bool EnumerateArchetypes(ref uint index, ulong signature, Archetype** archetypeOut)
    {
        while (index < _archetypes.Length)
        {
            var archetype = _archetypes.GetPointer(index++);
            if (archetype->Id.Signature % signature == 0)
            {
                *archetypeOut = archetype;
                return true;
            }
        }

        return false;
    }


    public CachedQuery ConstructQuery()
    {
        var comp1 = Transform3D.Type.Id;
        var comp2 = TransformRect.Type.Id;
        var numberOfComponents = 2;

        Span<uint> compIds = [comp1, comp2]
            ;
        compIds.Sort();

        var signature = comp1 * comp2;
        var archetypes = stackalloc Archetype*[1024];
        var offsets = stackalloc ushort[1024 * numberOfComponents];

        var count = 0;
        for (var index = 0; index < _archetypeCount; ++index)
        {
            var arch = _archetypes.GetPointer(index);
            if (arch->Id.Signature % signature == 0)
            {
                archetypes[count] = arch;
                var offsetStart = offsets + (count * numberOfComponents);
                var offsetIndex = 0;

                ref readonly var layout = ref arch->Layout;
                for (var i = 0; i < layout.NumberOfComponents; ++i)
                {
                    if (layout.Ids[i] == compIds[offsetIndex])
                    {
                        *(offsetStart + offsetIndex) = layout.Offsets[i];
                        offsetIndex++;
                    }
                }
                count++;
            }
        }

        return default;

    }
}
