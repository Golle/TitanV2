using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics;

public enum CullMode : byte
{
    None = D3D12_CULL_MODE.D3D12_CULL_MODE_NONE,
    Front = D3D12_CULL_MODE.D3D12_CULL_MODE_FRONT,
    Back = D3D12_CULL_MODE.D3D12_CULL_MODE_BACK,
}
