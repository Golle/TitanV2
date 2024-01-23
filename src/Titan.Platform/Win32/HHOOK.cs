using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("{Unused}")]
public struct HHOOK
{
    private int Unused;
}
