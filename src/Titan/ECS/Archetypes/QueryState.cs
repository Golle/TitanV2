namespace Titan.ECS.Archetypes;

/// <summary>
/// Keeps the state of the Query, only used for the internals in the Entity Systems
/// </summary>
public unsafe ref struct QueryState
{
    public int Count;
    internal uint Index;
    internal Chunk* Chunk;
}
