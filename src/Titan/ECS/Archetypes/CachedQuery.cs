using System.Runtime.CompilerServices;
using Titan.Core;

namespace Titan.ECS.Archetypes;

public unsafe struct CachedQuery
{
    private readonly Inline8<ComponentType> _components;
    private readonly ushort _componentCount;
    public readonly ReadOnlySpan<ComponentType> Components => _components.AsReadOnlySpan();
    public readonly ulong Signature;

    internal Archetype** Archetypes;
    internal ushort* Offsets;
    internal ushort Count;

    public CachedQuery(ReadOnlySpan<ComponentType> components, ulong signature)
    {
        components.CopyTo(_components.AsSpan());
        _componentCount = (ushort)components.Length;
        Signature = signature;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public bool EnumerateData(ref QueryState state, Entity** entities, void** data)
    {
        if (Count == 0)
        {
            return false;
        }

        while (state.Index < Count)
        {
            var archetype = Archetypes[state.Index];

            if (state.Chunk == null)
            {
                state.Chunk = archetype->ChunkStart;
            }

            var offsetStart = state.Index * _componentCount;
            //NOTE(Jens): This part could be source generated, but then the entire struct would need to be generated for each system. 
            //NOTE(Jens): Might create a better implementation if we put this method inside the system. The binary will be bigger, but the execution speed will increase. 
            //TODO(Jens): Test it out at some point. 
            for (var i = 0; i < _componentCount; ++i)
            {
                data[i] = state.Chunk->GetDataRow(Offsets[offsetStart + i]);
            }

            *entities = state.Chunk->GetEntities();
            state.Count = state.Chunk->Header.NumberOfEntities;
            state.Chunk = state.Chunk->Header.Next;

            if (state.Chunk == null)
            {
                state.Index++;
            }

            return true;
        }

        return false;
    }
}
