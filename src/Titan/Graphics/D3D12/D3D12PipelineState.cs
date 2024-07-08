using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics.D3D12;

internal struct D3D12PipelineState
{
    public PipelineState PipelineState;
    public ComPtr<ID3D12PipelineState> Resource;
}
