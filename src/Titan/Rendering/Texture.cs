using Titan.Graphics.D3D12.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering;

public struct Texture
{
    public uint Width;
    public uint Height;

    internal ComPtr<ID3D12Resource> Resource;
    internal D3D12DescriptorHandle SRV;
    internal D3D12DescriptorHandle RTV;
    internal D3D12DescriptorHandle DSV;
    internal DXGI_FORMAT Format;
}
