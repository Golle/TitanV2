using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

[StructLayout(LayoutKind.Explicit, Pack = 4)]
public struct Size(int width = 0, int height = 0)
{
    [FieldOffset(0)]
    public int Width = width;
    [FieldOffset(4)]
    public int Height = height;
    [FieldOffset(0)]
    public int X;
    [FieldOffset(4)]
    public int Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size operator +(in Size lh, in Size rh) => new(lh.Width + rh.Width, lh.Height + rh.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size operator -(in Size lh, in Size rh) => new(lh.Width - rh.Width, lh.Height - rh.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref Size lh, in Size rh)
    {
        lh.Width += rh.Width;
        lh.Height += rh.Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector2(in Size size) => new(size.Width, size.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SizeF(in Size size) => new(size.Width, size.Height);
}
