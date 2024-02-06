using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Point(int x, int y)
{
    public int X = x;
    public int Y = y;

    //TODO(Jens): Add more functions etc.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point operator +(in Point lh, in Point rh) => new(lh.X + rh.X, lh.Y + rh.Y);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point operator -(in Point lh, in Point rh) => new(lh.X - rh.X, lh.Y - rh.Y);


    public static explicit operator Vector2(in Point p) => new(p.X, p.Y);
}
