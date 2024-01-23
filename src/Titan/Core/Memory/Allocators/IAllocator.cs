namespace Titan.Core.Memory.Allocators;

public unsafe interface IAllocator
{
    static abstract void* Alloc(void* allocator, uint size);
    static abstract void Free(void* allocator, void* ptr);
}