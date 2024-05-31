using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Size(int width = 0, int height = 0)
{
    public int Width = width;
    public int Height = height;


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
}
