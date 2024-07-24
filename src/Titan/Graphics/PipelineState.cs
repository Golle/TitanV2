using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Graphics;

//TODO(Jens): Figure out if we should remove the extra D3D12 implementations of these, and just use a void* here.
public struct PipelineState
{
    internal ComPtr<ID3D12PipelineState> Resource;
}

