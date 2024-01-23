using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential)]
public struct BITMAPINFO
{
    public BITMAPINFOHEADER bmiHeader;
    public RGBQUADArray bmiColors;
}