using System.Diagnostics;

namespace Titan.Core;

public unsafe ref struct TitanBinaryReader(ReadOnlySpan<byte> buffer)
{
    private readonly ReadOnlySpan<byte> _buffer = buffer;
    private int _offset;
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

    public static implicit operator TitanBinaryReader(ReadOnlySpan<byte> buffer) => new(buffer);
}
