using System.Diagnostics;
using Titan.Core.Logging;
using Titan.Platform.Win32;

namespace Titan.Core.Memory.Platform;

public readonly unsafe struct Win32PlatformAllocator : IPlatformAllocator
{
    private static readonly uint _pageSize;

    static Win32PlatformAllocator()
    {
        _pageSize = GetPageSize();
    }
    public static uint GetPageSize()
    {
        SYSTEM_INFO info;
        Kernel32.GetSystemInfo(&info);
        return info.dwPageSize;
    }

    public static void* Reserve(void* startAddress, uint pages)
    {
        Logger.Trace<Win32PlatformAllocator>($"Reserve Virtual Memory. Pages = {pages} Size = {pages * _pageSize} bytes StartAddress = 0x{(nuint)startAddress:x8}");
        var mem = Kernel32.VirtualAlloc(startAddress, pages * _pageSize, AllocationType.MEM_RESERVE, AllocationProtect.PAGE_NOACCESS);
        Debug.Assert(mem != null);
        Logger.Trace<Win32PlatformAllocator>($"Reserved Virtual Memory. Pages = {pages} Address = 0x{(nuint)mem:x8}");
        return mem;
    }

    public static void Commit(void* startAddress, uint pages, uint pageOffset = 0)
    {
        Logger.Trace<Win32PlatformAllocator>($"Commit Virtual Memory. Pages = {pages}. PageOffset = {pageOffset} Address = 0x{(nuint)startAddress:x8}");
        var offset = pageOffset * _pageSize;
        var result = Kernel32.VirtualAlloc((byte*)startAddress + offset, pages * _pageSize, AllocationType.MEM_COMMIT, AllocationProtect.PAGE_READWRITE);
        Debug.Assert(result != null);
    }

    public static void Decommit(void* startAddress, uint pages, uint pageOffset = 0)
    {
        Logger.Trace<Win32PlatformAllocator>($"Decommit Virtual Memory. Pages = {pages}. PageOffset = {pageOffset} Address = 0x{(nuint)startAddress:x8}");
        var offset = pageOffset * _pageSize;
        var result = Kernel32.VirtualFree((byte*)startAddress + offset, pages * _pageSize, AllocationType.MEM_DECOMMIT);
        Debug.Assert(result);
    }

    public static void Release(void* startAddress, uint pages)
    {
        Logger.Trace<Win32PlatformAllocator>($"Release Virtual Memory. Pages = {pages}. Address = 0x{(nuint)startAddress:x8}");
        var result = Kernel32.VirtualFree(startAddress, 0 /*pages * PageSize*/, AllocationType.MEM_RELEASE);
        Debug.Assert(result);
    }
}