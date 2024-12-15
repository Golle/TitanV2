using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

public unsafe struct BumpAllocator(byte* mem, uint allocatorSize) : IAllocator
{
    private uint _offset;

    public static BumpAllocator From(TitanBuffer buffer) => new(buffer.AsPointer(), buffer.Size);
    public static void* Alloc(void* allocator, uint size)
    {
        var bump = (BumpAllocator*)allocator;
        return bump->Alloc(size);
    }

    public static unsafe void Free(void* allocator, void* ptr)
    {
        // no op for bump allocator
    }

    public static void Release(void* allocator, IMemoryManager memoryManager)
    {
        Debug.Assert(allocator != null);
        memoryManager.Free(allocator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Alloc<T>() where T : unmanaged => (T*)Alloc((uint)sizeof(T));
    public void* Alloc(uint size)
    {
        Debug.Assert(_offset + size <= allocatorSize);
        var ptr = mem + _offset;
        _offset += size;
        return ptr;
    }

    /// <summary>
    /// Allocates a <see cref="TitanArray{T}"/> of specified count.
    /// <remarks>Returns Empty array when count is 0</remarks>
    /// </summary>
    /// <typeparam name="T">The type to allocate</typeparam>
    /// <param name="count">The count</param>
    /// <returns>The allocated array</returns>
    public TitanArray<T> AllocateArray<T>(int count) where T : unmanaged
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
    public TitanArray<T> AllocateArray<T>(uint count) where T : unmanaged
    {
        if (count == 0)
        {
            return TitanArray<T>.Empty;
        }
        var size = (uint)(sizeof(T) * count);
        Debug.Assert(_offset + size <= allocatorSize);
        var array = new TitanArray<T>((T*)(mem + _offset), count);
        _offset += size;
        return array;
    }


    public TitanList<T> AllocateList<T>(uint count) where T : unmanaged
    {
        if (count == 0)
        {
            return TitanList<T>.Empty;
        }

        var size = (uint)(sizeof(T) * count);
        Debug.Assert(_offset + size <= allocatorSize);
        var start = mem + _offset;
        _offset += size;
        return new TitanList<T>((T*)start, count);
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

    public static implicit operator Allocator(in BumpAllocator allocator) => allocator.AsAllocator();
}
