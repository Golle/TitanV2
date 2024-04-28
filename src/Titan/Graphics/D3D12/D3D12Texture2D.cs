using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.Rendering;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12;

internal struct D3D12Texture2D
{
    public Texture2D Texture2D;
    public ComPtr<ID3D12Resource> Resource;
    public DescriptorHandle SRV;
    public DescriptorHandle RTV;
}
