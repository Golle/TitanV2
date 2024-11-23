using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct SizeF(float width = 0, float height = 0)
{
    public float Width = width;
    public float Height = height;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF operator +(in SizeF lh, in SizeF rh) => new(lh.Width + rh.Width, lh.Height + rh.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF operator -(in SizeF lh, in SizeF rh) => new(lh.Width - rh.Width, lh.Height - rh.Height);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF operator /(in SizeF lh, float value) => new(lh.Width / value, lh.Height / value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizeF operator *(in SizeF lh, float value) => new(lh.Width * value, lh.Height * value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref SizeF lh, in SizeF rh)
    {
        lh.Width += rh.Width;
        lh.Height += rh.Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector2(in SizeF size) => new(size.Width, size.Height);



}
