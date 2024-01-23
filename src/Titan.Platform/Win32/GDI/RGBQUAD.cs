using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RGBQUAD
{
    public byte rgbBlue;
    public byte rgbGreen;
    public byte rgbRed;
    public byte rgbReserved;
}