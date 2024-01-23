using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly unsafe struct TitanArray<T>(T* ptr, uint length)
    where T : unmanaged
{
    public readonly uint Length = length;
    public T* AsPointer() => ptr;
    public bool IsValid => ptr != null;
    public Span<T> AsSpan() => new(ptr, (int)Length);
    public ReadOnlySpan<T> AsReadOnlySpan() => new(ptr, (int)Length);

    public static implicit operator ReadOnlySpan<T>(in TitanArray<T> arr) => arr.AsSpan();


    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(index >= 0 && index < Length);
            return ref ptr[index];
        }
    }

    public ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Debug.Assert(index < Length);
            return ref ptr[index];
        }
    }
}