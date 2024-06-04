using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Memory;

namespace Titan.ECS.Archetypes;

[StructLayout(LayoutKind.Sequential, Size = (int)ChunkSize)]
internal unsafe struct Chunk
{
    /*
     * Chunk memory layout
     * <HEADER>
     * <ENTITIES> - dynamic count, based on components size
     * <COMPONENT1>
     * <COMPONENT2>
     * <COMPONENT3>
     */


    public const uint ChunkSize = 16 * 1024; // 16KB, 4 pages
    public static readonly uint DataSize = (uint)(ChunkSize - sizeof(ChunkHeader));
    public ChunkHeader Header;
    private byte _dataStart;
    private readonly byte* DataStart
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MemoryUtils.AsPointer(_dataStart);
    }

    private readonly Entity* Entities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Entity*)DataStart;
    }

    public readonly Entity* GetEntities() => (Entity*)MemoryUtils.AsPointer(in _dataStart);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void* GetComponentData(ushort offset, ushort size, uint index)
    {
        Debug.Assert(index < Header.NumberOfEntities);
        return DataStart + (offset + index * size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort Alloc(in Entity entity)
    {
        var index = Header.NumberOfEntities++;
        Entities[index] = entity;
        return index;
    }

    public (Entity Entity, ushort NewIndex) Free(ushort index, in ArchetypeLayout layout)
    {
        Debug.Assert(index < Header.NumberOfEntities);
        Header.NumberOfEntities--;
        if (index == Header.NumberOfEntities)
        {
            // last one, no data modification needed
            return default;
        }

        var lastComponentIndex = Header.NumberOfEntities;
        var entities = Entities;
        var data = DataStart;
        
        // move the last entity and save it for later use (needs to be returned so we can change the metadata for that entity)
        var movedEndity = entities[index] = entities[lastComponentIndex];
        for (var i = 0; i < layout.NumberOfComponents; ++i)
        {
            //NOTE(Jens): This requires NumberOfComponents + 1 * memcpys. 
            var size = layout.Sizes[i];
            var offset = layout.Offsets[i];
            var blockStart = data + offset;

            var target = blockStart + index * size;
            var source = blockStart + lastComponentIndex * size;
            MemoryUtils.Copy(target, source, size);
        }

        return (movedEndity, index);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChunkHeader
    {
        public Chunk* Next;
        public Chunk* Previous;
        public ushort NumberOfEntities;
        
    }
}
