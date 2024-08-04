using Titan.Core;
using Titan.Core.Maths;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

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
            Inputs = [BuiltInRenderTargets.DeferredLighting],
            Outputs = [BuiltInRenderTargets.Backbuffer],
            PixelShader = EngineAssetsRegistry.ShaderFullscreenPixel,
            VertexShader = EngineAssetsRegistry.ShaderFullscreenVertex,
            ClearFunction = &ClearBackbuffer
        });
    }

    private static void ClearBackbuffer(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
        => commandList.ClearRenderTargetView(renderTargets[0], Color.Magenta);

    [System]
    public static void Render(in BackbufferRenderPass pass, in RenderGraph graph, in Window window)
    {
        if (!graph.Begin(pass.PassHandle, out var commandList))
        {
            return;
        }

        D3D12_VIEWPORT viewPort = new()
        {
            Height = window.Height,
            Width = window.Width,
            MaxDepth = 1.0f,
            MinDepth = 0,
            TopLeftX = 0,
            TopLeftY = 0
        };
        commandList.SetViewport(&viewPort);

        D3D12_RECT rect = new()
        {
            Bottom = window.Height,
            Right = window.Width,
            Left = 0,
            Top = 0
        };
        commandList.SetScissorRect(&rect);

        commandList.DrawInstanced(3, 1);
        graph.End(pass.PassHandle);
    }
}
