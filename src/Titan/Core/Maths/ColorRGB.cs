using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

/// <summary>
/// A color value without the Alpha channel, can be packed in 3 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ColorRGB
{
    private const float ByteMax = byte.MaxValue;
    public float R, G, B;
    public ColorRGB(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }

    /// <summary>
    /// Create a new ColorRGB, alpha channel is ignored
    /// </summary>
    /// <param name="rgba">Red, Green, Blue, Alpha</param>
    public ColorRGB(uint rgba)
    {
        B = ((rgba >> 8) & 0xff) / ByteMax;
        G = ((rgba >> 16) & 0xff) / ByteMax;
        R = ((rgba >> 24) & 0xff) / ByteMax;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ColorRGB(in Color color) => new(color.R, color.G, color.B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(in ColorRGB color) => new(color.R, color.G, color.B);
}
