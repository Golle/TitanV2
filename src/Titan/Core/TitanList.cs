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

    public void Add(in T value)
    {
        var index = _next++;
        Debug.Assert(index < _elements.Length);
        _elements[index] = value;
    }

    public readonly ReadOnlySpan<T> AsReadOnlySpan() => new(_elements.AsPointer(), (int)_next);
    public readonly Span<T> AsSpan() => new(_elements.AsPointer(), (int)_next);

    public static implicit operator TitanList<T>(Span<T> data) => new(MemoryUtils.AsPointer(data.GetPinnableReference()), (uint)data.Length);

    public static implicit operator ReadOnlySpan<T>(in TitanList<T> list) => list.AsReadOnlySpan();
    public static implicit operator Span<T>(in TitanList<T> list) => list.AsSpan();

    public void Clear() => _next = 0;
}
