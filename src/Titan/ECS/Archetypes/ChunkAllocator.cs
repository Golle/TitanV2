using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;

namespace Titan.ECS.Archetypes;

internal unsafe struct ChunkAllocator
{
    private GeneralAllocator _allocator;

    private Entry* _freeList;
    private int _freeListCount;
    private int _allocatedCount;
    public bool Init(IMemoryManager memoryManager, uint maxChunks, uint preAllocatedChunks)
    {
        var maxSize = (uint)(maxChunks * sizeof(Chunk));
        var preAllocatedSize = (uint)(preAllocatedChunks * sizeof(Chunk));
        if (!memoryManager.TryCreateGeneralAllocator(out _allocator, maxSize, preAllocatedSize))
        {
            Logger.Error<ChunkAllocator>($"Failed to create the internal allocator. Max Chunks = {maxChunks} Max Size = {maxSize} bytes Pre Allocated Chunks = {preAllocatedChunks} Pre Allocated Size = {preAllocatedChunks * sizeof(Chunk)} bytes");
            return false;
        }

        for (var i = 0; i < preAllocatedChunks; ++i)
        {
            var chunk = (Entry*)_allocator.Alloc<Chunk>();
            chunk->Next = _freeList;
            _freeList = chunk;
        }

        _freeListCount = _allocatedCount = (int)preAllocatedChunks;

        return true;
    }

    public void Shutdown()
    {
        _allocator.Release();
    }

    /// <summary>
    /// Allocate a new Chunk, this is unitialized memory.
    /// </summary>
    /// <returns>The pointer to the chunk or null if out of memory</returns>
    public Chunk* Allocate()
    {
        var chunk = _freeList;
        if (chunk is not null)
        {
            _freeList = chunk->Next;
            _freeListCount--;
            return (Chunk*)chunk;
        }

        Logger.Trace<ChunkAllocator>("Out of pre-allocated chunks, allocating new one.");

        var newChunk = _allocator.Alloc<Chunk>();
        if (newChunk == null)
        {
            Logger.Error<ChunkAllocator>("Out of memory in allocator.");
        }

        _allocatedCount++;
        return newChunk;
    }

    /// <summary>
    /// Returns a chunk to the allocator
    /// </summary>
    /// <param name="chunk">The pointer to the chunk, must not be null</param>
    public void Free(Chunk* chunk)
    {
        Debug.Assert(chunk != null, "Trying to free a null chunk");
        VerifyNotDuplicate(chunk);

        var entry = (Entry*)chunk;

        entry->Next = _freeList;
        _freeList = entry;
        _freeListCount++;
    }

    private struct Entry
    {
        public Entry* Next;
    }

    [Conditional("DEBUG")]
    private void VerifyNotDuplicate(Chunk* chunk)
    {
        var freeList = _freeList;
        do
        {
            Debug.Assert((nuint)freeList != (nuint)chunk);
        } while ((freeList = _freeList->Next) != null);
    }
}
