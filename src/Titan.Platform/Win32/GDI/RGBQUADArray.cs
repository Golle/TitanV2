using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.GDI;

[InlineArray(1)]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RGBQUADArray
{
    public RGBQUAD _ref;
}