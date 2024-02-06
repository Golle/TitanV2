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

    unsafe T* Alloc<T>() where T : unmanaged;
    unsafe void* Alloc(uint size);
    unsafe void Free(void* ptr);
    void FreeArray<T>(ref TitanArray<T> array) where T : unmanaged;
    void FreeBuffer(ref TitanBuffer buffer);

    bool TryCreateGeneralAllocator(out GeneralAllocator allocator, uint minSize, uint preAllocatedSize = 0);
    bool TryCreateBumpAllocator(out BumpAllocator allocator, uint size);
    bool TryCreatePoolAllocator<T>(out BumpAllocator allocator, uint count) where T : unmanaged;

    
}
