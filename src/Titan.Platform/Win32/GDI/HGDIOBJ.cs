using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct HGDIOBJ
{
    private void* Value;
    public static implicit operator HGDIOBJ(nint value) => new() { Value = (void*)value };
}
