using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Memory;

namespace Titan.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct TitanList<T>(T* ptr, uint length)
    where T : unmanaged
{
    private readonly TitanArray<T> _elements = new(ptr, length);
    private uint _next;
    public uint Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _next;
    }

    public bool IsEmpty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Count == 0;
    }

    /// <summary>
    /// Adds an item to the list and increases the counter
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The index of the added element</returns>
    public uint Add(in T value)
    {
        var index = _next++;
        Debug.Assert(index < _elements.Length);
        _elements[index] = value;
        return index;
    }

    public readonly ReadOnlySpan<T> AsReadOnlySpan() => new(_elements.AsPointer(), (int)_next);
    public readonly Span<T> AsSpan() => new(_elements.AsPointer(), (int)_next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T* AsPointer() => _elements.AsPointer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T* GetPointer(uint index)
    {
        Debug.Assert(index < _next);
        return _elements.AsPointer() + index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T* GetPointer(int index)
    {
        Debug.Assert(index >= 0);
        Debug.Assert(index < _next);
        return _elements.AsPointer() + index;
    }

    public static implicit operator TitanList<T>(Span<T> data) => new(MemoryUtils.AsPointer(data.GetPinnableReference()), (uint)data.Length);

    public static implicit operator ReadOnlySpan<T>(in TitanList<T> list) => list.AsReadOnlySpan();
    public static implicit operator Span<T>(in TitanList<T> list) => list.AsSpan();
    public static implicit operator T*(in TitanList<T> list) => list.AsPointer();

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *GetPointer(index);
    }

    public ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *GetPointer(index);
    }

    public void Clear() => _next = 0;

    public static TitanList<T> Empty => default;
}
