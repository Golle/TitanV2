using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct HDC
{
    private void* _value;
}
