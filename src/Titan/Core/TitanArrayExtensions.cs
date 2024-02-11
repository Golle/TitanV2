using System.Runtime.CompilerServices;

namespace Titan.Core;

public static class TitanArrayExtensions
{


    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static bool ContainsAny<T>(this TitanArray<T> array, in TitanArray<T> other) where T : unmanaged
    {
        if (array.IsEmpty || other.IsEmpty)
        {
            return false;
        }

        for (var inner = 0; inner < array.Length; ++inner)
        {
            for (var outer = 0; outer < array.Length; ++outer)
            {
                if (inner == outer)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
