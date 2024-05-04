using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12;

public struct D3D12Buffer
{
    public Buffer Buffer;
    public ComPtr<ID3D12Resource> Resource;
    public uint StartOffset;
}