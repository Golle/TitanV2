namespace Titan.Core.Memory;

public unsafe interface IPlatformAllocator
{
    static abstract uint GetPageSize();
    static abstract void* Reserve(void* startAddress, uint pages);
    static abstract void Commit(void* startAddress, uint pages, uint pageOffset = 0);
    static abstract void Decommit(void* startAddress, uint pages, uint pageOffset = 0);
    static abstract void Release(void* startAddress, uint pages);
}