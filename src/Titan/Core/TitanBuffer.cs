using System.Runtime.InteropServices;

namespace Titan.Core;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly unsafe struct TitanBuffer(void* ptr, uint size)
{
    private readonly TitanArray<byte> _buffer = new((byte*)ptr, size);
    public uint Size => _buffer.Length;
    public byte* AsPointer() => _buffer.AsPointer();
    public bool IsValid => _buffer.IsValid;
    public ReadOnlySpan<byte> AsReadOnlySpan() => _buffer.AsSpan();
    public Span<byte> AsSpan() => _buffer.AsSpan();
    
    public static implicit operator ReadOnlySpan<byte>(in TitanBuffer buffer) => buffer.AsReadOnlySpan();
    public static implicit operator Span<byte>(in TitanBuffer buffer) => buffer.AsSpan();
}
