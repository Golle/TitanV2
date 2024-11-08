using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PROPVARIANT
{
    [FieldOffset(0)]
    public ushort vt;  // Variant type
    [FieldOffset(8)]
    public void* p;  // Pointer for data
}
