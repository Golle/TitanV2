using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32;

namespace Titan.Graphics;

public struct RootSignature
{
    internal ComPtr<ID3D12RootSignature> Resource;
}
