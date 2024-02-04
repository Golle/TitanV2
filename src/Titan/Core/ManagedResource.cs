using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core;

public struct ManagedResource<T>(GCHandle handle) : IDisposable
    where T : class
{
    public T Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(handle.IsAllocated);
            Debug.Assert(handle.Target != null);

            return (T)handle.Target!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get() => Value;

    public bool IsValid
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => handle is { IsAllocated: true, Target: not null };
    }


    /// <summary>
    ///  Allocate a new GCHandle for a managed resource so it can be used in unmanaged memory.
    /// </summary>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static ManagedResource<T> Alloc(T resource)
        => new(GCHandle.Alloc(resource));

    public void Dispose()
        => Release();

    public void Release()
    {
        if (handle.IsAllocated)
        {
            handle.Free();
        }
    }
}
