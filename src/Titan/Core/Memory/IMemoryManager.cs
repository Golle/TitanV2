using Titan.Core.Memory.Allocators;

namespace Titan.Core.Memory;

public interface IMemoryManager : IService
{
    /// <summary>
    /// Alloc an array from the global general purpose memory.
    /// <remarks>Dedicated allocators are always preferred</remarks>
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="count">Number of elements in the array</param>
    /// <returns>True if the allocation was successful</returns>
    bool TryAllocArray<T>(out TitanArray<T> array, uint count) where T : unmanaged;

    /// <summary>
    /// Alloc a buffer from the global general purpose memory.
    /// <remarks>Dedicated allocators are always preferred</remarks>
    /// </summary>
    /// <param name="buffer">The buffer</param>
    /// <param name="size">The size of the buffer</param>
    /// <returns>True if the allocation was successful</returns>
    bool TryAllocBuffer(out TitanBuffer buffer, uint size);

    /// <summary>
    /// Alloc an array from the global general purpose memory.
    /// <remarks>Dedicated allocators are always preferred</remarks>
    /// </summary>
    /// <param name="list">The list</param>
    /// <param name="count">Number of slots in the list</param>
    /// <returns>True if the allocation was successful</returns>
    bool TryAllocList<T>(out TitanList<T> list, uint count) where T : unmanaged;

    unsafe T* Alloc<T>() where T : unmanaged;
    unsafe void* Alloc(uint size);
    unsafe void Free(void* ptr);
    void FreeArray<T>(ref TitanArray<T> array) where T : unmanaged;
    void FreeBuffer(ref TitanBuffer buffer);

    bool TryCreateGeneralAllocator(out GeneralAllocator allocator, uint minSize, uint preAllocatedSize = 0);
    bool TryCreateBumpAllocator(out BumpAllocator allocator, uint size);
    bool TryCreateAtomicBumpAllocator(out AtomicBumpAllocator allocator, uint size);
    bool TryCreatePoolAllocator<T>(out PoolAllocator<T> allocator, uint count) where T : unmanaged;
    bool TryCreateResourcePool<T>(out ResourcePool<T> pool, uint count) where T : unmanaged;

    /// <summary>
    /// Helper method to release allocators created from the IMemmoryManager interface.
    /// </summary>
    /// <param name="allocator">The allocator (Only PoolAllocators can be released this way atm)</param>
    void FreeAllocator<T>(in T allocator) where T : unmanaged, IAllocator;

    void FreeResourcePool<T>(ref ResourcePool<T> pool) where T : unmanaged;
    internal bool Init(MemoryConfig config);
    internal void Shutdown();
}
