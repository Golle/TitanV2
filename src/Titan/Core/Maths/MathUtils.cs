using System.Numerics;
using System.Runtime.CompilerServices;

namespace Titan.Core.Maths;

public static class MathUtils
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOf2(uint value)
        => value > 0 && (value & (value - 1u)) == 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CalculateWorldMatrix(ref Matrix4x4 matrix, in Vector3 position, in Vector3 scale, in Quaternion rotation)
    {
        matrix = Matrix4x4.CreateFromQuaternion(rotation);

        matrix.M11 *= scale.X;
        matrix.M12 *= scale.X;
        matrix.M13 *= scale.X;

        matrix.M21 *= scale.Y;
        matrix.M22 *= scale.Y;
        matrix.M23 *= scale.Y;

        matrix.M31 *= scale.Z;
        matrix.M32 *= scale.Z;
        matrix.M33 *= scale.Z;

        matrix.M41 = position.X;
        matrix.M42 = position.Y;
        matrix.M43 = position.Z;

    }
}
