using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.ECS.Archetypes;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct EntityData
{
    public ArchetypeRecord Record;
    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Record.Archetype != null && Record.Chunk != null;
    }
}

internal unsafe struct ArchetypeRecord
{
    public ushort Index;
    public Archetype* Archetype;
    public Chunk* Chunk;
}

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

        for (var i = 0; i < _archetypeCount; i++)
        {
            if (_archetypes[i].Id.Signature == signature)
            {
                return _archetypes.GetPointer(i);
            }
        }
        return null;
    }

    public void RemoveComponent(in Entity entity, in ComponentType type)
    {

    }

}

internal unsafe struct Archetype
{
    public readonly ArchetypeId Id;
    public readonly ArchetypeLayout Layout;
    public readonly ChunkAllocator* Allocator;
    private readonly Chunk* _chunkStart;

    public int ChunkCount;
    public int EntitiesCount;

    public Archetype(in ArchetypeId id, ChunkAllocator* allocator)
    {
        Debug.Assert(id.ComponentsSize > 0);
        Debug.Assert(allocator != null);

        Id = id;
        Layout = new ArchetypeLayout(id);
        Allocator = allocator;
        _chunkStart = AllocAndInitChunk();
    }

    public ArchetypeRecord Add(Entity entity)
    {
        EntitiesCount++;
        var chunk = GetChunk();
        var index = chunk->Alloc(entity);
        return new()
        {
            Archetype = MemoryUtils.AsPointer(this),
            Chunk = chunk,
            Index = index
        };
    }

    /// <summary>
    /// Removes the record from the archetype. This could cause another entity to be moved, if that's the case the entity and new record will be returned.
    /// </summary>
    /// <param name="record">The record to remove</param>
    /// <returns>The entity and record that changed. Invalid entity returned if no move occurred.</returns>
    public (Entity Entity, ArchetypeRecord Record) Remove(in ArchetypeRecord record)
    {
        EntitiesCount--;
        //TODO: this can be optimized a bit, since in some cases there is no move. we need to determine that early. Maybe use out variable?

        var result = record.Chunk->Free(record.Index, Layout);
        return (result.Entity, record with { Index = result.NewIndex });
    }

    private Chunk* GetChunk()
    {
        var chunk = _chunkStart;
        Debug.Assert(chunk != null, "No chunk in the archetype, this is an invalid state there should always be one.");

        while (chunk->Header.NumberOfEntities >= Layout.EntitiesPerChunk)
        {
            if (chunk->Header.Next != null)
            {
                chunk = chunk->Header.Next;
            }
            else
            {
                chunk->Header.Next = AllocAndInitChunk();
            }
        }
        
        return chunk;
    }

    private Chunk* AllocAndInitChunk()
    {
        var chunk = Allocator->Allocate();
        var header = &chunk->Header;
        header->Next = null;
        header->NumberOfEntities = 0;
        ChunkCount++;
        return chunk;
    }
}
