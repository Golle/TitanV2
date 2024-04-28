using System.Runtime.InteropServices;
using Titan.Platform.Win32.DXGI;

namespace Titan.Assets.Types;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct Texture2DDescriptor
{
    public uint Width;
    public uint Height;
    public ushort Stride;
    public ushort BitsPerPixel;
    public DXGI_FORMAT DXGIFormat;
}