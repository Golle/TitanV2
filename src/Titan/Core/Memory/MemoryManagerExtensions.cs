using System.Diagnostics;

namespace Titan.Core.Memory;

public static class MemoryManagerExtensions
{
    public static bool TryAllocArray<T>(this IMemoryManager memoryManager, out TitanArray<T> array, int count) where T : unmanaged
    {
        Debug.Assert(count >= 0);
        return memoryManager.TryAllocArray(out array, (uint)count);
    }
}
