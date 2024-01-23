using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct SIZE
{
    public int X;
    public int Y;
}
