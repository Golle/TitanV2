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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator +(Vector3Int a, int value)
        => new(a.X + value, a.Y + value, a.Z + value);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator +(Vector3Int a, Vector3Int value)
        => new(a.X + value.X, a.Y + value.Y, a.Z + value.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int a, int value)
        => new(a.X - value, a.Y - value, a.Z - value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Clamp(Vector3Int v, Vector3Int min, Vector3Int max) =>
        Vector128.Clamp(v.AsVector128(), min.AsVector128(), max.AsVector128())
            .AsVector3Int();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Max(Vector3Int v1, Vector3Int v2)
        => Vector128.Max(v1.AsVector128(), v2.AsVector128())
            .AsVector3Int();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Min(Vector3Int v1, Vector3Int v2)
        => Vector128.Min(v1.AsVector128(), v2.AsVector128())
            .AsVector3Int();

    public static readonly Vector3Int Zero = default;
}




public static class MathUtils
{

    // BEGIN CHAT GPT generated code
    // Revisit this and make sure it's as optimal as it can be.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SmoothStep(float edge0, float edge1, float x)
    {
        // Clamp x to the 0-1 range
        float t = Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3.0f - 2.0f * t); // Cubic smoothing
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(float value) 
        => MathF.Max(0.0f, MathF.Min(1.0f, value));

    // END CHAT GPT generated code

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector128<int> AsVector128(this Vector3Int value)
    {
        Unsafe.SkipInit(out Vector128<int> result);
        Unsafe.WriteUnaligned(ref Unsafe.As<Vector128<int>, byte>(ref result), value);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int AsVector3Int(this Vector128<int> value)
    {
        ref var address = ref Unsafe.As<Vector128<int>, byte>(ref value);
        return Unsafe.ReadUnaligned<Vector3Int>(ref address);
    }

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
