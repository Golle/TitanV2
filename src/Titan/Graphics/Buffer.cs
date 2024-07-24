using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32;

namespace Titan.Graphics;

public struct Buffer
{
    public uint Count;
    public uint Stride;
    public BufferType Type;
    public uint Size => Count * Stride;


    // D3D12
    internal ComPtr<ID3D12Resource> Resource;
    internal uint StartOffset;
}
