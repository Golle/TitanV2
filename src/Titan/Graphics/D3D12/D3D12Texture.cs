using Titan.Graphics.D3D12.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12;

internal struct D3D12Texture
{
    public Texture Texture;
    public ComPtr<ID3D12Resource> Resource;
    public DescriptorHandle SRV;
    public DescriptorHandle RTV;
    public DescriptorHandle DSV;
}
