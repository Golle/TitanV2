using System.Runtime.CompilerServices;

namespace Titan.Core.Memory;

internal unsafe struct PlatformAllocator(uint pageSize)
{
    public readonly uint PageSize = pageSize;
    private delegate*<void*, uint, void*> _reserve;
    private delegate*<void*, uint, uint, void> _commit;
    private delegate*<void*, uint, uint, void> _decommit;
    private delegate*<void*, uint, void> _release;

    public static PlatformAllocator Create<T>() where T : IPlatformAllocator =>
        new(T.GetPageSize())
        {
            _reserve = &T.Reserve,
            _commit = &T.Commit,
            _decommit = &T.Decommit,
            _release = &T.Release
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void* Reserve(void* startAddress, uint pages) => _reserve(startAddress, pages);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Commit(void* startAddress, uint pages, uint pageOffset = 0) => _commit(startAddress, pages, pageOffset);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Decommit(void* startAddress, uint pages, uint pageOffset = 0) => _decommit(startAddress, pages, pageOffset);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Release(void* startAddress, uint pages) => _release(startAddress, pages);
}