using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;

namespace Titan.Core.Memory;

public record MemoryConfig(uint MaxVirtualMemory, uint GeneralPurposeMemory) : IConfiguration, IDefault<MemoryConfig>
{
    public static readonly uint DefaultMaxVirtualMemory = MemoryUtils.MegaBytes(512);
    public static readonly uint DefaultGeneralPurposeMemory = MemoryUtils.MegaBytes(128);

    public static MemoryConfig Default => new(DefaultMaxVirtualMemory, DefaultGeneralPurposeMemory);
}

internal sealed unsafe class MemorySystem<TPlatformAllocator> : IMemorySystem where TPlatformAllocator : IPlatformAllocator
{
    private PlatformAllocator* _allocator;
    private VirtualMemory _globalMemory;
    private GeneralAllocator _generalAllocator;
    private readonly object _syncObject = new();

    public bool Init(in MemoryConfig config)
    {
        Debug.Assert(config.GeneralPurposeMemory < config.MaxVirtualMemory, $"{nameof(MemoryConfig.MaxVirtualMemory)} must be greater than {nameof(MemoryConfig.GeneralPurposeMemory)}.");


        _allocator = (PlatformAllocator*)MemoryUtils.GlobalAlloc((nuint)sizeof(PlatformAllocator));
        if (_allocator == null)
        {
            Logger.Error<MemorySystem<TPlatformAllocator>>($"Failed to allocate memory for the {nameof(PlatformAllocator)}");
            return false;
        }
        *_allocator = PlatformAllocator.Create<TPlatformAllocator>();

        if (!VirtualMemory.TryCreate(out _globalMemory, _allocator, config.MaxVirtualMemory))
        {
            Logger.Error<MemorySystem<TPlatformAllocator>>($"Failed to Reserve {nameof(VirtualMemory)}. Size = {config.MaxVirtualMemory} bytes.");
            return false;
        }

        if (!_globalMemory.TryReserveBlock(config.GeneralPurposeMemory, out var generalPurposeMemory))
        {
            Logger.Error<MemorySystem<TPlatformAllocator>>($"Failed to reserve memory for the General Purpose Allocator. Size = {config.GeneralPurposeMemory} bytes.");
            return false;
        }

        _generalAllocator = new GeneralAllocator(generalPurposeMemory);

        return true;
    }

    public void Shutdown()
    {
        _generalAllocator.Release();
        _globalMemory.Release();
        if (_allocator != null)
        {
            MemoryUtils.GlobalFree(_allocator);
        }
    }

    public bool TryAllocArray<T>(out TitanArray<T> array, uint count) where T : unmanaged
    {
        lock (_syncObject)
        {
            return _generalAllocator.TryAllocArray(out array, count, true);
        }
    }

    public bool TryAllocBuffer(out TitanBuffer buffer, uint size)
    {
        lock (_syncObject)
        {
            return _generalAllocator.TryAllocBuffer(out buffer, size, true);
        }
    }

    public T* Alloc<T>() where T : unmanaged
        => (T*)Alloc((uint)sizeof(T));
    public void* Alloc(uint size)
    {
        lock (_syncObject)
        {
            return _generalAllocator.Alloc(size, true);
        }
    }

    public void Free(void* ptr)
    {
        lock (_syncObject)
        {
            _generalAllocator.Free(ptr);
        }
    }

    public void FreeArray<T>(ref TitanArray<T> array) where T : unmanaged
    {
        lock (_syncObject)
        {
            _generalAllocator.FreeArray(ref array);
        }
    }

    public void FreeBuffer(ref TitanBuffer buffer)
    {
        lock (_syncObject)
        {
            _generalAllocator.FreeBuffer(ref buffer);
        }
    }

    public bool TryCreateGeneralAllocator(out GeneralAllocator allocator, uint minSize, uint preAllocatedSize = 0)
    {
        Unsafe.SkipInit(out allocator);

        lock (_syncObject)
        {
            if (!_globalMemory.TryReserveBlock(minSize, out var block))
            {
                Logger.Error<MemorySystem<TPlatformAllocator>>($"Failed to create a {nameof(GeneralAllocator)}. Size = {minSize} bytes");
                return false;
            }
            allocator = new GeneralAllocator(block);
        }
        return true;
    }

    public bool TryCreateBumpAllocator(out BumpAllocator allocator, uint size)
    {
        throw new NotImplementedException();
    }

    public bool TryCreatePoolAllocator<T>(out BumpAllocator allocator, uint count) where T : unmanaged
    {
        throw new NotImplementedException();
    }
}
