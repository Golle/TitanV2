using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Titan.Core.Maths;

public record struct Vector3Int(int X, int Y, int Z)
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Round(in Vector3 vector)
    {
        var x = (int)(vector.X + 0.5f);
        var y = (int)(vector.Y + 0.5f);
        var z = (int)(vector.Z + 0.5f);
        return new(x, y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3(in Vector3Int vector) => new(vector.X, vector.Y, vector.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3Int(in Vector3 vector) => new((int)vector.X, (int)vector.Y, (int)vector.Z);
}




public static class MathUtils
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Floor(in Vector3 vector) => new Vector3Int((int)MathF.Floor(vector.X), (int)MathF.Floor(vector.Y), (int)MathF.Floor(vector.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPowerOf2(uint value)
        => value > 0 && (value & (value - 1u)) == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(float start, float end, float value)
        => (value - start) / (end - start);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float end, float t)
        => end * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float start, float end, float t)
        => start + (end - start) * t;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWithin(in Vector2 position, in SizeF size, in Point point)
    {
        if (position.X > point.X)
        {
            return false;
        }

        if (position.Y > point.Y)
        {
            return false;
        }

        if (position.Y + size.Height < point.Y)
        {
            return false;
        }

        if (position.X + size.Width < point.X)
        {
            return false;
        }
        return true;

    }


    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector4 Multiply(in Matrix4x4 matrix, in Vector4 vector) =>
        new(
            matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z + matrix.M14 * vector.W,
            matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z + matrix.M24 * vector.W,
            matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z + matrix.M34 * vector.W,
            matrix.M41 * vector.X + matrix.M42 * vector.Y + matrix.M43 * vector.Z + matrix.M44 * vector.W
        );
}
