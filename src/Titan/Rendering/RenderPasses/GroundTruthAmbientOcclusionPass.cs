using Titan.Assets;
using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct GroundTruthAmbientOcclusionPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(ref GroundTruthAmbientOcclusionPass pass, in RenderGraph renderGraph, in D3D12ResourceManager resourceManager)
    {
        //TODO(Jens): Implement Compute Shaders later.
        pass.PassHandle = renderGraph.CreatePass(nameof(GroundTruthAmbientOcclusionPass), new()
        {
            BlendState = BlendStateType.Disabled,
            CullMode = CullMode.Back,
            
            Outputs = [
                BuiltInRenderTargets.AmbientOcclusion
            ],
            Inputs =
            [
                BuiltInRenderTargets.GBufferNormal,
            ],
            Shaders = [
                new ()
                {
                    VertexShader = EngineAssetsRegistry.Shaders.ShaderFullscreenVertex,
                    PixelShader = EngineAssetsRegistry.Shaders.ShaderGTAOPixel
                }
            ],
            DepthBuffer = BuiltInDepthsBuffers.GbufferDepthBuffer,
            DepthBufferMode = DepthBufferMode.Read,
            ClearFunction = &ClearFunction,
            //RootSignatureBuilder = static builder => builder
            //    .WithConstant(3, ShaderVisibility.Pixel, register: 0, space: 0)
            //    .WithDecriptorRange(1, register: 0, space: 0),
        });
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.AmbientOcclusion.OptimizedClearColor);
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(ref GroundTruthAmbientOcclusionPass pass, in RenderGraph renderGraph, in D3D12ResourceManager resourceManager)
    {
        if (!renderGraph.Begin(pass.PassHandle, out var commandList))
        {
            return;
        }


    }

    [System]
    public static void Render(in GroundTruthAmbientOcclusionPass pass, in RenderGraph renderGraph)
    {
        if (!renderGraph.IsReady)
        {
            return;
        }

        var commandList = renderGraph.GetCommandList(pass.PassHandle);
        commandList.DrawInstanced(3, 1);
        renderGraph.End(pass.PassHandle);
    }
}
