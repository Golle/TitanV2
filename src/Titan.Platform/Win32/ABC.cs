using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct ABC
{
    public int abcA;
    public uint abcB;
    public int abcC;
}
