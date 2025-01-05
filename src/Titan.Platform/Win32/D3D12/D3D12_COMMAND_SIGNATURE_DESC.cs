using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.D3D12;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct D3D12_COMMAND_SIGNATURE_DESC
{
    public uint ByteStride;
    public uint NumArgumentDescs;
    public D3D12_INDIRECT_ARGUMENT_DESC* pArgumentDescs;
    public uint NodeMask;
}
