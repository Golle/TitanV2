using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Memory;

namespace Titan.Core.Strings;

//TODO(Jens): Implement Source generators for this. 

[StructLayout(LayoutKind.Sequential, Size = Size)]
public unsafe struct CStringW128
{
    public const int Size = 128;
    private char _ref;
    public CStringW128(ReadOnlySpan<char> str)
    {
        var length = Math.Min(Size - 1, str.Length);
        MemoryUtils.Copy(Unsafe.AsPointer(ref _ref), str[..length]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator char*(in CStringW128 str) => MemoryUtils.AsPointer(in str._ref);
}

[StructLayout(LayoutKind.Sequential, Size = Size)]
public unsafe struct CStringW256
{
    public const int Size = 256;
    private char _ref;
    public CStringW256(ReadOnlySpan<char> str)
    {
        var length = Math.Min(Size - 1, str.Length);
        MemoryUtils.Copy(Unsafe.AsPointer(ref _ref), str[..length]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator char*(in CStringW256 str) => MemoryUtils.AsPointer(in str._ref);
}


[StructLayout(LayoutKind.Sequential, Size = Size)]
public unsafe struct CStringA128
{
    public const int Size = 128;
    private byte _ref;
    public CStringA128(ReadOnlySpan<byte> str)
    {
        var length = Math.Min(Size - 1, str.Length);
        MemoryUtils.Copy(Unsafe.AsPointer(ref _ref), str[..length]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator byte*(in CStringA128 str) => MemoryUtils.AsPointer(in str._ref);
}

