using Titan.Assets;
using Titan.Core.Ids;

namespace Titan.Rendering;

public record RenderPipeline
{
    public required RenderPipelinePass[] RenderPasses { get; init; }
}
public sealed record RenderPipelineConfiguration : IConfiguration, IDefault<RenderPipelineConfiguration>
{
    public required Func<RenderPipeline> PipelineConfigurationBuilder { get; init; }
    public static RenderPipelineConfiguration Default => new()
    {
        PipelineConfigurationBuilder = DefaultRenderPipelineBuilder.Build
    };
}

public record RenderPipelinePass(string Identifier)
{
    public required RenderPassType Type { get; init; }
    public required RenderPipelineRenderTarget[] Inputs { get; init; }
    public required RenderPipelineRenderTarget[] Outputs { get; init; }
    public RenderPipelineDepthBuffer? DepthBufferOutput { get; init; }
    public RenderPipelineDepthBuffer? DepthBufferInput { get; init; }
    public required AssetDescriptor Shader { get; init; }

    //TODO(Jens): Add support for SubPasses, this could be useful for DepthOfField or MotionBlur (yuk) and maybe other post-processing effects
    //public RenderPipelinePass[] SubPasses { get; init; } = [];
}
public record RenderPipelineRenderTarget(string Identifier, RenderTargetFormat Format)
{
    public int Id { get; } = IdGenerator<RenderPipelineRenderTarget, int, SimpleValueIncrement<int>>.GetNext();
    public static readonly RenderPipelineRenderTarget Backbuffer = new("Backbuffer", RenderTargetFormat.BackBuffer);
}
public record RenderPipelineDepthBuffer(string Identifier, DepthBufferFormat Format, float ClearValue = 1.0f);

public enum RenderTargetFormat
{
    RGBA8,
    BackBuffer
}

public enum DepthBufferFormat
{
    F32
}


public interface IRenderingPipelineBuilder
{
    static abstract RenderPipeline Build();
}
