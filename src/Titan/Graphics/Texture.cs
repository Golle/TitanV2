using Titan.Graphics.D3D12.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;

namespace Titan.Graphics;

public struct Texture
{
    public uint Width;
    public uint Height;

    internal ComPtr<ID3D12Resource> Resource;
    internal DescriptorHandle SRV;
    internal DescriptorHandle RTV;
    internal DescriptorHandle DSV;
    internal DXGI_FORMAT Format;
}
