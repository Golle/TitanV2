using Titan.Core;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct DeferredLightingRenderPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(DeferredLightingRenderPass* renderPass, in RenderGraph graph)
    {
        var rootSignatureArgs = new RootSignatureBuilder()
            .WithRanges(6, space: 10)
            .WithSampler(SamplerState.Point, ShaderVisibility.Pixel)
            .Build();

        renderPass->PassHandle = graph.CreatePass("DeferredLighting", new()
        {
            RootSignature = rootSignatureArgs,
            Outputs = [BuiltInRenderTargets.DeferredLighting],
            Inputs =
            [
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular
            ],
            PixelShader = EngineAssetsRegistry.ShaderDeferredLightingPixel,
            VertexShader = EngineAssetsRegistry.ShaderDeferredLightingVertex
        });
    }

    [System]
    public static void Render(in DeferredLightingRenderPass pass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.Begin(pass.PassHandle, out var commandList))
        {
            return;
        }

        //commandList.ClearRenderTargetView();

        commandList.DrawInstanced(3, 1);

        graph.End(pass.PassHandle);
    }
}
