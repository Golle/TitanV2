using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Rendering;

public struct RootSignature
{
    internal ComPtr<ID3D12RootSignature> Resource;
}
