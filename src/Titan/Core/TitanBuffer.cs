using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Titan.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly unsafe struct TitanBuffer(void* ptr, uint bufferSize)
{
    public static readonly TitanBuffer Empty = default;
    private readonly TitanArray<byte> _buffer = new((byte*)ptr, bufferSize);
    public uint Size => _buffer.Length;
    public byte* AsPointer() => _buffer.AsPointer();
    public bool IsValid => _buffer.IsValid;
    public ReadOnlySpan<byte> AsReadOnlySpan() => _buffer.AsSpan();
    public Span<byte> AsSpan() => _buffer.AsSpan();

    /// <summary>
    /// Slice the buffer with an offset, returning the remaining bytes as a new buffer.
    /// </summary>
    /// <param name="offset">Offset in the buffer</param>
    /// <returns>A buffer with the new size (same memory)</returns>
    public TitanBuffer Slice(uint offset) 
        => Slice(offset, _buffer.Length - offset);

    public TitanBuffer Slice(uint offset, uint size)
    {
        if (size == 0)
        {
            return Empty;
        }

        Debug.Assert(offset < _buffer.Length);
        Debug.Assert(offset + size <= Size);
        return new(AsPointer() + offset, size);
    }

    public TitanArray<T> SliceArray<T>(uint offset, uint count) where T : unmanaged
    {
        if (count == 0)
        {
            return TitanArray<T>.Empty;
        }

        var totalSize = sizeof(T) * count;
        Debug.Assert(offset < Size);
        Debug.Assert(offset + totalSize <= Size);
        var start = AsPointer() + offset;

        return new((T*)start, count);
    }

    public static implicit operator ReadOnlySpan<byte>(in TitanBuffer buffer) => buffer.AsReadOnlySpan();
    public static implicit operator Span<byte>(in TitanBuffer buffer) => buffer.AsSpan();
}
