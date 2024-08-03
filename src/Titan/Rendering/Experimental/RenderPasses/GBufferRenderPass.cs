using Titan.Assets;
using Titan.Core;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.Experimental.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct GBufferRenderPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(GBufferRenderPass* renderPass, in RenderGraph renderGraph, in AssetsManager assetsManager)
    {
        var rootSignatureArgs = new RootSignatureBuilder()
            .WithConstantBuffer(ConstantBufferFlags.Static, space: 10)
            .WithConstant(4, ShaderVisibility.Pixel, register: 0, space: 11)
            .WithRanges(6, register: 0, space: 0)
            .WithSampler(SamplerState.Linear, ShaderVisibility.Pixel)
            .WithSampler(SamplerState.Point, ShaderVisibility.Pixel)
            .Build();

        var passArgs = new CreateRenderPassArgs
        {
            RootSignature = rootSignatureArgs,
            Outputs =
            [
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular,
            ],
            Inputs = [],
            VertexShader = EngineAssetsRegistry.ShaderGBufferVertex,
            PixelShader = EngineAssetsRegistry.ShaderGBufferPixel
        };

        renderPass->PassHandle = renderGraph.CreatePass("GBuffer", passArgs);
    }

    [System]
    public static void CollectData(GBufferRenderPass* pass, ReadOnlySpan<Mesh3D> meshes)
    {

    }

    [System]
    public static void RecordCommandList(GBufferRenderPass* pass, in RenderGraph graph)
    {
        if (!graph.Begin(pass->PassHandle, out var commandList))
        {
            return;
        }

        commandList.DrawInstanced(3, 1);

        graph.End(pass->PassHandle);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(GBufferRenderPass* pass)
    {

    }
}
