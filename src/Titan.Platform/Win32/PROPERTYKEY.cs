using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]
public struct PROPERTYKEY
{
    public Guid fmtid;
    public uint pid;
}
