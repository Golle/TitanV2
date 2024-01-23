using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

public struct PoolAllocator<T> : IAllocator where T : unmanaged
{
    public static unsafe void* Alloc(void* allocator, uint size)
    {
        throw new NotImplementedException();
    }

    public static unsafe void Free(void* allocator, void* ptr)
    {
        throw new NotImplementedException();
    }
    public readonly Allocator AsAllocator() => Allocator.Create(ref Unsafe.AsRef(in this));

    public static implicit operator Allocator(in PoolAllocator<T> allocator) => allocator.AsAllocator();
}