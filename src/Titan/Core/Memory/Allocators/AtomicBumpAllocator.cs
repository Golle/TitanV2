using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

public unsafe struct AtomicBumpAllocator(byte* mem, uint allocatorSize) : IAllocator
{
    private uint _offset;
    public static void* Alloc(void* allocator, uint size)
    {
        var bump = (AtomicBumpAllocator*)allocator;
        return bump->Alloc(size);
    }

    public static void Free(void* allocator, void* ptr)
    {
        throw new NotImplementedException();
    }

    public static void Release(void* allocator, IMemoryManager memoryManager)
    {
        Debug.Assert(allocator != null);
        memoryManager.Free(allocator);
    }

    public readonly void* Alloc(int size)
    {
        Debug.Assert(size >= 0);
        return Alloc((uint)size);
    }

    public readonly void* Alloc(uint size)
    {
        var end = Interlocked.Add(ref Unsafe.AsRef(in _offset), size);
        var ptr = mem + end - size;
        Debug.Assert(end <= allocatorSize);
        return ptr;
    }

    /// <summary>
    /// Allocates a <see cref="TitanArray{T}"/> of specified count.
    /// <remarks>Returns Empty array when count is 0</remarks>
    /// </summary>
    /// <typeparam name="T">The type to allocate</typeparam>
    /// <param name="count">The count</param>
    /// <returns>The allocated array</returns>
    public readonly TitanArray<T> AllocateArray<T>(int count) where T : unmanaged
    {
        Debug.Assert(count >= 0);
        return AllocateArray<T>((uint)count);
    }

    /// <summary>
    /// Allocates a <see cref="TitanArray{T}"/> of specified count.
    /// <remarks>Returns Empty array when count is 0</remarks>
    /// </summary>
    /// <typeparam name="T">The type to allocate</typeparam>
    /// <param name="count">The count</param>
    /// <returns>The allocated array</returns>
    public readonly TitanArray<T> AllocateArray<T>(uint count) where T : unmanaged
    {
        if (count == 0)
        {
            return TitanArray<T>.Empty;
        }
        var size = (uint)(sizeof(T) * count);
        var end = Interlocked.Add(ref Unsafe.AsRef(in _offset), size);
        Debug.Assert(end <= allocatorSize);

        var ptr = mem + end - size;
        var array = new TitanArray<T>((T*)ptr, count);

        return array;
    }

    public void Reset(bool initialize = false)
    {
        _offset = 0;
        if (initialize)
        {
            MemoryUtils.Init(mem, allocatorSize);
        }
    }

    public readonly Allocator AsAllocator() => Allocator.Create(ref Unsafe.AsRef(in this));

    public static implicit operator Allocator(in AtomicBumpAllocator allocator) => allocator.AsAllocator();
}
