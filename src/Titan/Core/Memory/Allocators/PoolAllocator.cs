using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Memory.Allocators;

public unsafe struct PoolAllocator<T> : IAllocator where T : unmanaged
{
    private static readonly uint Alignment = (uint)Math.Max(sizeof(T), sizeof(FreeItem));

    private readonly T* _memory;
    private readonly uint _count;
    private FreeItem* _freeList;
    private SpinLock _lock;

    /// <summary>
    /// Constructs a PoolAllocator from a memory pointer with a max count. 
    /// </summary>
    /// <param name="mem">The pointer to the pre-allocated unmanaged memory</param>
    /// <param name="count">The number of elements in the pool</param>
    public PoolAllocator(T* mem, uint count)
    {
        Debug.Assert(sizeof(T) >= sizeof(FreeItem));

        for (var i = 0; i < count; ++i)
        {
            var next = _freeList;
            _freeList = (FreeItem*)(mem + i);
            _freeList->Next = next;
        }

        _memory = mem;
        _count = count;
    }

    /// <summary>
    /// Allocates a resource from this pool
    /// </summary>
    /// <returns>The resource or null if the max amount has been exceeded.</returns>
    public T* Alloc()
    {
        var item = _freeList;
        if (item == null)
        {
            return null;
        }

        _freeList = _freeList->Next;
        return (T*)item;
    }

    /// <summary>
    /// Releases a resource that was allocated with this allocator. Runtime checks only happens in debug builds.
    /// </summary>
    /// <param name="value">The resource</param>
    public void Free(T* value)
    {
        Debug.Assert(value >= _memory && value < _memory + _count);

        var next = _freeList;
        _freeList = (FreeItem*)value;
        _freeList->Next = next;
    }

    /// <summary>
    /// Same as <see cref="Alloc()"/> but with a SpinLock
    /// </summary>
    /// <returns>The ptr to the object or null</returns>
    public T* SafeAlloc()
    {
        var token = false;
        _lock.Enter(ref token);
        try
        {
            return Alloc();
        }
        finally
        {
            _lock.Exit();
        }
    }

    /// <summary>
    /// Same as <see cref="Free(T*)"/> but with a SpinLock
    /// </summary>
    public void SafeFree(T* value)
    {
        var token = false;
        _lock.Enter(ref token);
        try
        {
            Free(value);
        }
        finally
        {
            _lock.Exit();
        }
    }

    public static void* Alloc(void* allocator, uint size)
    {
        Debug.Assert(size == Alignment);

        throw new NotImplementedException("Not sure how to do this since the base one is generic");
    }

    public static unsafe void Free(void* allocator, void* ptr)
    {
        throw new NotImplementedException("Not sure how to do this since the base one is generic");
    }

    public static void Release(void* allocator, IMemoryManager memoryManager)
    {
        //NOTE(Jens): The memory is the first member in the struct, we can treat the allocator as a void** to get that pointer and release it.
        var ptr = *(void**)allocator;
        *(void**)allocator = null; // Yep, we'll overwrite the readonly value :)
        memoryManager.Free(ptr);
    }

    public readonly Allocator AsAllocator() => Allocator.Create(ref Unsafe.AsRef(in this));

    public static implicit operator Allocator(in PoolAllocator<T> allocator) => allocator.AsAllocator();

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    private struct FreeItem
    {
        public FreeItem* Next;
    }
}
