using System.Runtime.InteropServices;

namespace Titan.ECS.Archetypes;



internal struct ArchetypeLayout
{
    public ComponentId Components;

    public uint Stride;


}

internal struct Archetype
{
    public ArchetypeLayout Layout;




}

[StructLayout(LayoutKind.Explicit, Size = (int)ChunkSize)]
internal struct Chunk
{
    public const uint ChunkSize = 16 * 1024; // 16KB, 4 pages


}




internal struct Component
{

}
