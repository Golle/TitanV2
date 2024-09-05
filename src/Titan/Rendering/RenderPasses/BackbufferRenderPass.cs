using Titan.Core;
using Titan.Core.Maths;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

file struct BackbufferData
{
    public Color Color;
}

[UnmanagedResource]
internal unsafe partial struct BackbufferRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint DataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    [System(SystemStage.Init)]
    public static void Init(BackbufferRenderPass* renderPass, in RenderGraph graph)
    {
        renderPass->PassHandle = graph.CreatePass("BackbufferRenderPass", new()
        {
            RootSignatureBuilder = static builder => builder.WithRootConstant<BackbufferData>(register: 1, space: 7),
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
        commandList.SetGraphicsRootConstant(DataIndex, new BackbufferData
        {
            Color = Color.Green
        });

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
