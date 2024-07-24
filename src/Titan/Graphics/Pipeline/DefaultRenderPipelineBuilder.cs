namespace Titan.Graphics.Pipeline;

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
