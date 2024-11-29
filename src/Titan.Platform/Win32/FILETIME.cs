using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct FILETIME
{
    public int dwLowDateTime;
    public int dwHighDateTime;
}
