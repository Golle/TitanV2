using Kepler.Core.Memory;
using Titan.Core.Logging;
using Titan.Core.Memory.Platform;

namespace Titan.Core.Memory;

internal unsafe struct VirtualMemory
{
    private readonly PlatformAllocator* _allocator;
    private readonly void* _startAddress;
    private readonly uint _reservedPages;
    private uint _claimedPages;

    private VirtualMemory(PlatformAllocator* allocator, void* startAddress, uint reservedPages)
    {
        _allocator = allocator;
        _startAddress = startAddress;
        _reservedPages = reservedPages;
    }

    public static bool TryCreate(out VirtualMemory memory, PlatformAllocator* allocator, nuint minReserveSize, ulong startAddress = 0)
    {
        memory = default;
        var pages = (uint)MemoryUtils.AlignToUpper(minReserveSize, allocator->PageSize) / allocator->PageSize;
        var memoryBlock = allocator->Reserve((void*)startAddress, pages);
        if (memoryBlock == null)
        {
            Logger.Error<VirtualMemory>($"Failed to reserve {pages} pages (size: {pages * allocator->PageSize} bytes)");
            return false;
        }
        memory = new VirtualMemory(allocator, memoryBlock, pages);
        return true;
    }


    public bool TryReserveBlock(nuint minReserveSize, out VirtualMemoryBlock block)
    {
        block = default;
        var alignedReserveBytes = MemoryUtils.AlignToUpper(minReserveSize, _allocator->PageSize);
        var pages = (uint)alignedReserveBytes / _allocator->PageSize;
        if (_claimedPages + pages > _reservedPages)
        {
            Logger.Error<VirtualMemory>("Trying to reserve a bigger block than fits the original reservation.");
            return false;
        }
        var mem = (byte*)_startAddress + _claimedPages * _allocator->PageSize;
        block = new VirtualMemoryBlock(_allocator, mem, pages);
        _claimedPages += pages;

        return true;
    }

    public void Release()
    {
        if (_startAddress != null && _allocator != null)
        {
            _allocator->Release(_startAddress, _reservedPages);
        }
    }
}
