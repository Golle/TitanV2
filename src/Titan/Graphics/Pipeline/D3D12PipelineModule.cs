using Titan.Application;
using Titan.Assets;
using Titan.Core.Ids;

namespace Titan.Graphics.Pipeline;

internal sealed class D3D12PipelineModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12PipelineStateObjectRegistry>()
            .AddSystemsAndResource<Graph.D3D12RenderGraph>()
            //.AddSystemsAndResource<D3D12RenderGraph1>()
            ;

        return true;
    }
}

public interface IRenderingPipelineBuilder
{
    static abstract RenderPipeline Build();
}

public sealed class DefaultRenderPipelineBuilder : IRenderingPipelineBuilder
{
    public static RenderPipeline Build()
    {
        RenderPipelineRenderTarget gbufferAlbedo = new("GBuffer_Albedo", RenderTargetFormat.RGBA8);
        RenderPipelineRenderTarget gbufferNormal = new("GBuffer_Normal", RenderTargetFormat.RGBA8);
        RenderPipelineRenderTarget gbufferSpecular = new("GBuffer_Specular", RenderTargetFormat.RGBA8);
        RenderPipelineDepthBuffer depthBuffer = new("GBuffer_Depth", DepthBufferFormat.F32);

        RenderPipelinePass gBufferRenderPass = new("GBuffer")
        {
            Type = RenderPassType.Scene,
            Inputs = [],
            Outputs = [gbufferAlbedo, gbufferNormal, gbufferSpecular],
            DepthBufferOutput = depthBuffer,
            Shader = EngineAssetsRegistry.ShaderGBuffer,
        };

        RenderPipelineRenderTarget lighting = new("DeferredLighting_RenderTarget", RenderTargetFormat.RGBA8);
        //RenderPipelineRenderTarget forward = new("Forward_RenderTarget", RenderTargetFormat.RGBA8);

        RenderPipelinePass lightingPass = new("DeferredLighting")
        {
            Type = RenderPassType.DeferredLighting,
            Shader = EngineAssetsRegistry.ShaderDeferredLighting,
            Inputs = [gbufferAlbedo, gbufferNormal, gbufferSpecular],
            Outputs = [lighting],
            DepthBufferOutput = null,
            DepthBufferInput = depthBuffer
        };

        //RenderPipelineDepthBuffer forwardDepthBuffer = new("Forward_Depth", DepthBufferFormat.F32);
        //RenderPipelinePass forwardRenderPass = new("Forward")
        //{
        //    Type = RendererTypes.FullScreen,
        //    VertexShader = EngineAssetsRegistry.FullScreenVertexShader,
        //    PixelShader = EngineAssetsRegistry.FullScreenPixelShader,
        //    Inputs = [lighting],
        //    Outputs = [forward],
        //    DepthBuffer = forwardDepthBuffer,
        //};
        //RenderPipelineRenderTarget debugTarget = new("Debug_RT", RenderTargetFormat.RGBA8);
        //RenderPipelinePass debugPass = new("Debug")
        //{
        //    Inputs = [],
        //    Outputs = [debugTarget],
        //    Shader = default,
        //    Type = RenderPassType.Custom
        //};


        RenderPipelinePass finalPass = new("Final")
        {
            Type = RenderPassType.Backbuffer,
            Inputs = [lighting/*, debugTarget*/],
            Outputs = [RenderPipelineRenderTarget.Backbuffer],
            Shader = EngineAssetsRegistry.ShaderFullscreen
        };

        return new RenderPipeline
        {
            RenderPasses =
            [
                gBufferRenderPass,
                lightingPass,
                //forwardRenderPass,
                //debugPass,
                finalPass
            ]
        };
    }
}

public enum RenderPassType
{
    /// <summary>
    /// Only a single scene pass can exist
    /// </summary>
    Scene,

    /// <summary>
    /// Only a single DeferredLighting pass can exist
    /// </summary>
    DeferredLighting,

    /// <summary>
    /// Only a single Backbuffer pass can exist
    /// </summary>
    Backbuffer,

    /// <summary>
    /// The custom render pass type can be used when lookups should be based on the name.
    /// </summary>
    Custom
}

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
