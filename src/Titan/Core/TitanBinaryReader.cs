
using System.Diagnostics;

namespace Titan.Core;

public unsafe ref struct TitanBinaryReader(ReadOnlySpan<byte> buffer)
{
    private readonly ReadOnlySpan<byte> _buffer = buffer;
    private int _offset;
    public bool HasData => _offset < _buffer.Length;

    public ref readonly T Read<T>() where T : unmanaged
    {
        var size = sizeof(T);
        Debug.Assert(_offset + size <= _buffer.Length);
        fixed (byte* bufferPtr = _buffer)
        {
            var data = (T*)(bufferPtr + _offset);
            _offset += size;
            return ref *data;
        }
    }

    public ReadOnlySpan<byte> Read(uint length)
        => Read((int)length);

    public ReadOnlySpan<byte> Read(int length)
    {
        Debug.Assert(_offset + length <= _buffer.Length);
        var span = _buffer.Slice(_offset, length);
        _offset += length;
        return span;
    }

    public void Advance(int bytes) => Read(bytes);
    public ReadOnlySpan<byte> GetRemaining() => _buffer[_offset..];

    public static implicit operator TitanBinaryReader(ReadOnlySpan<byte> buffer) => new(buffer);
}
