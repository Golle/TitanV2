using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;
using static Titan.Assets.EngineAssetsRegistry.Shaders;

namespace Titan.Rendering.RenderPasses;

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
            RootSignatureBuilder = static builder => builder.WithRootConstant<uint>(register: 1, space: 7),
            BlendState = BlendStateType.AlphaBlend,
            Inputs = [BuiltInRenderTargets.DeferredLighting, BuiltInRenderTargets.UI],
            Outputs = [BuiltInRenderTargets.Backbuffer],
            PixelShader = ShaderFullscreenPixel,
            VertexShader = ShaderFullscreenVertex,
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

        // draw World
        commandList.SetGraphicsRootConstant(DataIndex, 0u);
        commandList.DrawInstanced(3, 1);

        // draw UI
        commandList.SetGraphicsRootConstant(DataIndex, 1u);
        commandList.DrawInstanced(3, 1);

        graph.End(pass.PassHandle);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(BackbufferRenderPass* pass, in RenderGraph graph, in DXGISwapchain _) //NOTE(Jens): Get a Swapchain reference to make sure everything has been flushed before releasing it. A hack.. Need a better system for doing this.
    {
        Logger.Warning<BackbufferRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }
}
