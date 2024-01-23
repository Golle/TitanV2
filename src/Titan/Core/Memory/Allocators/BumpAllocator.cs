using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

public struct BumpAllocator : IAllocator
{
    //NOTE(Jens): Not implemented yet

    public static unsafe void* Alloc(void* allocator, uint size)
    {
        var bump = (BumpAllocator*)allocator;

        return null;
    }

    public static unsafe void Free(void* allocator, void* ptr)
    {
        var bump = (BumpAllocator*)allocator;
        // no op for bump allocator

    }

    public readonly Allocator AsAllocator() => Allocator.Create(ref Unsafe.AsRef(in this));

    public static implicit operator Allocator(in BumpAllocator allocator) => allocator.AsAllocator();
}