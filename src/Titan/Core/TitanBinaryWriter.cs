using System.Diagnostics;
using Titan.Core.Memory;

namespace Titan.Core;

public ref struct TitanBinaryWriter(Span<byte> buffer)
{
    private readonly Span<byte> _buffer = buffer;
    private int _length;
    public readonly int Length => _length;


    public void WriteBoolAsByte(bool value) => Write((byte)(value ? 1 : 0));
    public void WriteByte(byte value) => Write(value);
    public void WriteUint(uint value) => Write(value);
    public void WriteInt(int value) => Write(value);

    public void WriteString(string value)
    {
        throw new NotImplementedException("Yeah, not implemented yet :)");
    }
    public void WriteShort(short value) => Write(value);
    public unsafe void Write<T>(in T value) where T : unmanaged
    {
        var size = sizeof(T);
        Debug.Assert(_length + size <= _buffer.Length);
        fixed (T* ptr = &value)
        fixed (byte* bufferPtr = _buffer)
        {
            MemoryUtils.Copy(bufferPtr + _length, ptr, (uint)size);
        }
        _length += size;
    }
    public void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        var length = bytes.Length;
        Debug.Assert(_length + length <= _buffer.Length);

        bytes.CopyTo(_buffer[_length..]);
        _length += length;
    }

    public ReadOnlySpan<byte> GetData() => _buffer[.._length];

    public static implicit operator TitanBinaryWriter(Span<byte> buffer) => new(buffer);
}
