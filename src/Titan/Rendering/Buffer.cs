using Titan.Graphics.D3D12.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Rendering;

public struct Buffer
{
    public uint Count;
    public uint Stride;
    public BufferType Type;
    public uint Size => Count * Stride;


    // D3D12
    internal ComPtr<ID3D12Resource> Resource;
    internal uint StartOffset;
    internal D3D12DescriptorHandle SRV;
}
