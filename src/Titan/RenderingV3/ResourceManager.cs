using Titan.Core;
using Titan.Graphics.D3D12;
using Titan.Rendering;

namespace Titan.RenderingV3;

public unsafe struct ResourceManager
{
    private D3D12Context* _context;
    private D3D12ResourceManager1* _resources;

    public Handle<Texture> CreateTexture()
    {
        throw new NotImplementedException();

    }

    public Handle<PipelineState> CreatePipelineState(in CreatePipelineStateArgs args)
    {
        return _resources->CreatePipelineState(_context, args);
    }

    public Handle<GPUBuffer> CreateBuffer(in CreateBufferArgs args)
    {
        return default;
    }

}
