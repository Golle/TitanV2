using System.Diagnostics;

namespace Titan.Core;

public ref struct TitanBitReader(ReadOnlySpan<byte> buffer)
{
    private readonly ReadOnlySpan<byte> _buffer = buffer;
    private int _offset;

    public int ReadBits(int count)
    {
        Debug.Assert(count <= sizeof(int) * 8, "can't read more than 32 bits, because reasons.");
        var value = 0;
        for (var i = 0; i < count; ++i)
        {
            var bytePos = _offset / 8;
            var bitPos = _offset % 8;
            var bit = (_buffer[bytePos] >> bitPos) & 1;
            value |= (bit << i);
            _offset++;
        }
        return value;
    }

    public bool ReadBitAsBool() => ReadBits(1) == 1;
    public int GetBytesRead()
    {
        //Debug.Assert(_offset % 8 == 0, "The read bytes is not a multiple of 8. bug?");
        return _offset / 8;
    }

    public void SkipBits(int times)
    {
        Debug.Assert(_offset  + times < _buffer.Length*8);
        _offset += times;
    }
}
