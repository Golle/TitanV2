using System.Runtime.CompilerServices;

namespace Titan.Core.Memory.Allocators;

/// <summary>
/// Can hold any type of allocator. Should be used as argument to methods
/// Figure out how to implement this :)
/// </summary>
public unsafe ref struct Allocator
{
    private void* _allocator;
    private delegate*<void*, uint, void*> _alloc;
    private delegate*<void*, void*, void> _free;

    public static Allocator Create<T>(ref T context) where T : unmanaged, IAllocator =>
        new()
        {
            _allocator = Unsafe.AsPointer(ref context),
            _alloc = &T.Alloc,
            _free = &T.Free
        };

    public void* Alloc(uint size) => _alloc(_allocator, size);
    public void Free(void* ptr) => _free(_allocator, ptr);
}