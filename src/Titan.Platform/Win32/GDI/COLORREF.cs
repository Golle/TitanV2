using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct COLORREF
{
    public uint Value;
    public static COLORREF FromRGB(byte r, byte g, byte b) => new() { Value = ((uint)r | ((uint)g << 8) | ((uint)b << 16)) };
}