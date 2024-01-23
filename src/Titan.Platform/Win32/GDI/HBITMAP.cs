using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct HBITMAP
{
    private void* Value;

    public static implicit operator nint(in HBITMAP bitmap) => (nint)bitmap.Value;
    public static implicit operator HGDIOBJ(HBITMAP bitmap) => (nint)bitmap;
}
