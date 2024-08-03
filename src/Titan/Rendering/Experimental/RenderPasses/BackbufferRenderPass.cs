using Titan.Core;
using Titan.Core.Maths;
using Titan.Graphics.D3D12;
using Titan.Graphics;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.Experimental.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct BackbufferRenderPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(BackbufferRenderPass* renderPass, in RenderGraph graph)
    {
        var rootSignatureArgs = new RootSignatureBuilder()
            .WithRanges(6, space: 10)
            .WithSampler(SamplerState.Point, ShaderVisibility.Pixel)
            .Build();

        renderPass->PassHandle = graph.CreatePass("BackbufferRenderPass", new()
        {
            RootSignature = rootSignatureArgs,
            Outputs = [BuiltInRenderTargets.Backbuffer],
            Inputs = [BuiltInRenderTargets.DeferredLighting],
            PixelShader = EngineAssetsRegistry.ShaderFullscreenPixel,
            VertexShader = EngineAssetsRegistry.ShaderFullscreenVertex
        });
    }

    [System]
    public static void Render(in BackbufferRenderPass pass, in RenderGraph graph)
    {
        if (!graph.Begin(pass.PassHandle, out var commandList))
        {
            return;
        }

        commandList.DrawInstanced(3, 1);
        graph.End(pass.PassHandle);
    }
}
