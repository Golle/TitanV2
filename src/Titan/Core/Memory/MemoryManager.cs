using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Logging;
using Titan.Core.Memory.Allocators;

namespace Titan.Core.Memory;

public record MemoryConfig(uint MaxVirtualMemory, uint GeneralPurposeMemory) : IConfiguration, IDefault<MemoryConfig>
{
    public static readonly uint DefaultMaxVirtualMemory = MemoryUtils.GigaBytes(2);
    public static readonly uint DefaultGeneralPurposeMemory = MemoryUtils.MegaBytes(128);

    public static MemoryConfig Default => new(DefaultMaxVirtualMemory, DefaultGeneralPurposeMemory);
}

internal sealed unsafe class MemoryManager<TPlatformAllocator> : IMemoryManager where TPlatformAllocator : IPlatformAllocator
{
    private PlatformAllocator* _allocator;
    private VirtualMemory _globalMemory;
    private GeneralAllocator _generalAllocator;
    private readonly object _syncObject = new();

    public bool Init(MemoryConfig config)
    {
        Debug.Assert(config.GeneralPurposeMemory < config.MaxVirtualMemory, $"{nameof(MemoryConfig.MaxVirtualMemory)} must be greater than {nameof(MemoryConfig.GeneralPurposeMemory)}.");

        _allocator = (PlatformAllocator*)MemoryUtils.GlobalAlloc((nuint)sizeof(PlatformAllocator));
        if (_allocator == null)
        {
            Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to allocate memory for the {nameof(PlatformAllocator)}");
            return false;
        }
        *_allocator = PlatformAllocator.Create<TPlatformAllocator>();

        if (!VirtualMemory.TryCreate(out _globalMemory, _allocator, config.MaxVirtualMemory))
        {
            Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to Reserve {nameof(VirtualMemory)}. Size = {config.MaxVirtualMemory} bytes.");
            return false;
        }

        if (!_globalMemory.TryReserveBlock(config.GeneralPurposeMemory, out var generalPurposeMemory))
        {
            Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to reserve memory for the General Purpose Allocator. Size = {config.GeneralPurposeMemory} bytes.");
            return false;
        }

        _generalAllocator = new GeneralAllocator(generalPurposeMemory);

        return true;
    }

    public void Shutdown()
    {
        lock (_syncObject)
        {
            _generalAllocator.Release();
        }
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
                Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to create a {nameof(GeneralAllocator)}. Size = {minSize} bytes");
                return false;
            }
            allocator = new GeneralAllocator(block);
        }
        return true;
    }

    public bool TryCreateBumpAllocator(out BumpAllocator allocator, uint size)
    {
        //NOTE(Jens): maybe we want a way to allocate the context for the allocator as well, so we don't use "this" when create the Allocator for general usage. 
        Unsafe.SkipInit(out allocator);
        lock (_syncObject)
        {
            var mem = _generalAllocator.Alloc(size);
            if (mem == null)
            {
                Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to create a {nameof(BumpAllocator)}. Size = {size} bytes");

                return false;
            }

            allocator = new((byte*)mem, size);
            return true;
        }
    }

    public bool TryCreateAtomicBumpAllocator(out AtomicBumpAllocator allocator, uint size)
    {
        Unsafe.SkipInit(out allocator);
        lock (_syncObject)
        {
            var mem = _generalAllocator.Alloc(size);
            if (mem == null)
            {
                Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to create a {nameof(AtomicBumpAllocator)}. Size = {size} bytes");

                return false;
            }

            allocator = new((byte*)mem, size);
            return true;
        }
    }

    public bool TryCreatePoolAllocator<T>(out PoolAllocator<T> allocator, uint count) where T : unmanaged
    {
        Unsafe.SkipInit(out allocator);
        lock (_syncObject)
        {
            var size = (uint)sizeof(T) * count;
            var mem = (T*)_generalAllocator.Alloc(size);
            if (mem == null)
            {
                Logger.Error<MemoryManager<TPlatformAllocator>>($"Failed to create a {nameof(PoolAllocator<T>)}. Size = {size} bytes");
                return false;
            }

            allocator = new(mem, count);
            return true;
        }
    }

    public bool TryCreateResourcePool<T>(out ResourcePool<T> pool, uint count) where T : unmanaged
        => ResourcePool<T>.TryCreate(out pool, this, count);

    public void FreeResourcePool<T>(ref ResourcePool<T> pool) where T : unmanaged 
        => ResourcePool<T>.Destroy(ref pool, this);

    public void FreeAllocator<T>(in T allocator) where T : unmanaged, IAllocator
        => T.Release(MemoryUtils.AsPointer(in allocator), this);
}
