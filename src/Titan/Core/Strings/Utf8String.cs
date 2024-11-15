using System;
using System.Runtime.CompilerServices;

namespace Titan.Core.Strings;

public unsafe struct Utf8String(byte* ptr, uint length)
{
    private readonly byte* _ptr = ptr;
    private readonly uint _length = length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Utf8String lh, in Utf8String rh)
    {
        if (lh._length != rh._length)
        {
            return false;
        }

        for (var i = 0; i < lh._length; ++i)
        {
            if (rh._ptr[i] != lh._ptr[i])
            {
                return false;
            }
        }
        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Utf8String lh, Utf8String rh) => !(lh == rh);

    public override bool Equals(object? obj)
        => throw new NotSupportedException("Equals is not supported, use == or !=.");

    public override int GetHashCode() => throw new NotSupportedException("HashCode is not supported");
}
