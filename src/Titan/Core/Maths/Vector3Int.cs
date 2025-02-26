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
    public static Vector3Int operator -(Vector3Int a, Vector3Int value)
        => new(a.X - value.X, a.Y - value.Y, a.Z - value.Z);

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
    public static readonly Vector3Int One = new(1, 1, 1);
}
