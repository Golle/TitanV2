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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(uint index)
        => GetPointer((int)index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetPointer(int index)
    {
        Debug.Assert(index >= 0 && index < Length);
        return ptr + index;
    }

    /// <summary>
    /// Is Valid will check the underlying pointer and not the length. This should be used if accessing raw memory.
    /// <remarks>Use IsEmpty if you want to check if the array is empty. </remarks>
    /// </summary>
    public bool IsValid => ptr != null;

    public TitanList<T> AsList() => new(ptr, Length);

    public Span<T> AsSpan() => new(ptr, (int)Length);

    public ReadOnlySpan<T> AsReadOnlySpan() => new(ptr, (int)Length);

    public static implicit operator ReadOnlySpan<T>(in TitanArray<T> arr) => arr.AsSpan();
    /// <summary>
    /// Returns true for unitialized arrays and also empty arrays.
    /// <remarks>Length == 0 is Empty, no matter if the pointer is valid or not.</remarks>
    /// </summary>
    public bool IsEmpty => Length == 0;

    public static TitanArray<T> Empty => default;

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

    public TitanArray<T> Slice(uint offset, uint count)
    {
        if (count == 0)
        {
            return Empty;
        }
        Debug.Assert(offset < Length);
        Debug.Assert(offset + count <= Length);
        return new(ptr + offset, count);
    }

    public TitanBuffer AsBuffer()
        => new(ptr, (uint)(sizeof(T) * Length));
}
