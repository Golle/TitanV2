using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

internal unsafe struct Archetype
{
    public readonly ArchetypeId Id;
    public readonly ArchetypeLayout Layout;
    public readonly ChunkAllocator* Allocator;
    public Chunk* ChunkStart;

    public int ChunkCount;
    public int EntitiesCount;

    public Archetype(in ArchetypeId id, ChunkAllocator* allocator)
    {
        Debug.Assert(id.ComponentsSize > 0);
        Debug.Assert(allocator != null);

        Id = id;
        Layout = new ArchetypeLayout(id);
        Allocator = allocator;
        ChunkStart = AllocAndInitChunk();
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
        //NOTE(Jens): there's a risk of fragmentation here. We never move entities between chunks. This structure supports it but since we call Free on the Chunk it's not possible. The free have to be handled from outside (or we pass the target chunk to the the method)

        EntitiesCount--;
        //TODO: this can be optimized a bit, since in some cases there is no move. we need to determine that early. Maybe use out variable?

        var result = record.Chunk->Free(record.Index, Layout);
        if (record.Chunk->Header.NumberOfEntities == 0 && ChunkCount > 1)
        {
            ReleaseChunk(record.Chunk);
        }
        return (result.Entity, record with { Index = result.NewIndex });
    }

    private void ReleaseChunk(Chunk* chunk)
    {
        if (ChunkStart == chunk)
        {
            // special case where we have to update the archetypes start
            ChunkStart = chunk->Header.Next;
            ChunkStart->Header.Previous = null;
        }
        else
        {
            var next = chunk->Header.Next;
            var previous = chunk->Header.Previous;
            if (next != null)
            {
                next->Header.Previous = previous;
            }
            previous->Header.Next = next;
        }
        Allocator->Free(chunk);
        ChunkCount--;
    }

    private Chunk* GetChunk()
    {
        var chunk = ChunkStart;
        Debug.Assert(chunk != null, "No chunk in the archetype, this is an invalid state there should always be one.");

        while (chunk->Header.NumberOfEntities >= Layout.EntitiesPerChunk)
        {
            if (chunk->Header.Next != null)
            {
                chunk = chunk->Header.Next;
            }
            else
            {
                var newChunk = AllocAndInitChunk();
                newChunk->Header.Previous = chunk;
                chunk->Header.Next = newChunk;
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
