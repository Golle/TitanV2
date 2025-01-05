using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics;

public enum FillMode : byte
{
    Solid = D3D12_FILL_MODE.D3D12_FILL_MODE_SOLID,
    Wireframe = D3D12_FILL_MODE.D3D12_FILL_MODE_WIREFRAME
}
