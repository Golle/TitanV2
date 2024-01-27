using System.Runtime.CompilerServices;

namespace Titan.Core.Maths;

public static class MathUtils
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOf2(uint value)
        => value > 0 && (value & (value - 1u)) == 0;
}
