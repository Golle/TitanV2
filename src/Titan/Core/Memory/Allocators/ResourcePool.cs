using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core.Logging;

namespace Titan.Core.Memory.Allocators;

public unsafe struct ResourcePool<T> where T : unmanaged
{
    private static readonly uint Size = (uint)sizeof(T);
    private static readonly uint ResourceSize = (uint)sizeof(ResourceItem);
    private readonly TitanArray<ResourceItem> _resources;
    private SpinLock _lock;
    private ResourcePool(TitanArray<ResourceItem> resources)
    {
        _resources = resources;
    }
    public static bool TryCreate(out ResourcePool<T> pool, IMemoryManager memoryManager, uint count)
    {
        Unsafe.SkipInit(out pool);
        if (!memoryManager.TryAllocArray<ResourceItem>(out var array, count))
        {
            Logger.Error<ResourcePool<T>>($"Failed to allocate the array for the resource pool. Count = {count} Size = {ResourceSize} Total Size = {count * ResourceSize}");
            return false;
        }

        pool = new ResourcePool<T>(array);

        return true;
    }

    public readonly Handle<T> SafeAlloc()
    {
        Lock();
        try
        {
            //NOTE(Jens): We skip the first resource, or we'd have to add a number to the handle to prevent invalid handle from being returned. 0 = invalid.
            for (var i = 1; i < _resources.Length; ++i)
            {
                if (!_resources[i].InUse)
                {
                    _resources[i].InUse = true;
                    return i;
                }
            }
            return Handle<T>.Invalid;
        }
        finally
        {
            Exit();
        }
    }


    public readonly void SafeFree(Handle<T> handle)
    {
        if (handle.IsInvalid)
        {
            return;
        }
        Lock();

        try
        {
            ref var resource = ref _resources[handle];
            Debug.Assert(resource.InUse, "Trying to free a resource that is not in Use.");
            resource.InUse = false;
        }
        finally
        {
            Exit();
        }
    }

    public readonly bool TryAlloc(out Handle<T> handle)
    {
        throw new NotImplementedException();
    }

    public readonly ref readonly T AsRef(Handle<T> handle)
    {
        throw new NotImplementedException();
    }

    public readonly T* AsPtr(Handle<T> handle)
    {
        Debug.Assert(handle.IsValid);
        Debug.Assert(handle.Value < _resources.Length);
        var ptr = _resources.AsPointer() + handle.Value;
        return (T*)ptr;
    }

    private struct ResourceItem
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public T Value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        public bool InUse;
    }

    public static void Destroy(ref ResourcePool<T> pool, IMemoryManager memoryManager)
        => memoryManager.FreeArray(ref Unsafe.AsRef(in pool._resources));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void Lock()
    {
        var gotLock = false;
        Unsafe.AsRef(in _lock).Enter(ref gotLock);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void Exit() => Unsafe.AsRef(in _lock).Exit();
}
