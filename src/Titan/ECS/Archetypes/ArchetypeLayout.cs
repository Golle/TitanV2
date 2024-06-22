using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Titan.ECS.Archetypes;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
internal unsafe struct ArchetypeLayout
{
    public const int MaxComponents = 8;
    public readonly uint EntitiesPerChunk;
    public readonly uint NumberOfComponents;
    public fixed uint Ids[MaxComponents];
    public fixed ushort Offsets[MaxComponents];
    public fixed ushort Sizes[MaxComponents];
    public ArchetypeLayout(in ArchetypeId id)
    {
        var components = id.GetComponents();
        NumberOfComponents = (uint)components.Length;
        Debug.Assert(NumberOfComponents > 0, "Can't create archetype layout with no components");
        EntitiesPerChunk = Chunk.ChunkSize / (uint)(id.ComponentsSize + sizeof(Entity));

        // Offset the layout buy the amount of entities
        var offset = EntitiesPerChunk * sizeof(Entity);

        for (var i = 0; i < NumberOfComponents; ++i)
        {
            ref readonly var componentType = ref components[i];
            Ids[i] = componentType.Id;
            Sizes[i] = (ushort)componentType.Size;
            Offsets[i] = (ushort)offset;
        
            // Increase the offset with the component Size times the number of entities in the chunk.
            offset += componentType.Size * EntitiesPerChunk;
        }
    }
}
