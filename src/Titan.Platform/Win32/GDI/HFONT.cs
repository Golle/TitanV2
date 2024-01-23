using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[StructLayout(LayoutKind.Sequential)]
public struct HFONT
{
    public nint Value;

    public static implicit operator nint(in HFONT font) => font.Value;
    public static implicit operator HGDIOBJ(HFONT font) => (nint)font;
}
