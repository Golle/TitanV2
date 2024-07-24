using Titan.Graphics.Pipeline;

namespace Titan.Graphics;

public interface IRenderingPipelineBuilder
{
    static abstract RenderPipeline Build();
}
