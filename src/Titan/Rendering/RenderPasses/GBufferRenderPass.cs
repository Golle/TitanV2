using Titan.Assets;
using Titan.Core;
using Titan.Core.Maths;
using Titan.ECS.Components;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct GBufferRenderPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(GBufferRenderPass* renderPass, in RenderGraph renderGraph, in AssetsManager assetsManager)
    {
        var passArgs = new CreateRenderPassArgs
        {
            Outputs =
            [
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular,
            ],
            Inputs = [],
            ClearFunction = &ClearFunction,
            VertexShader = EngineAssetsRegistry.ShaderGBufferVertex,
            PixelShader = EngineAssetsRegistry.ShaderGBufferPixel
        };

        renderPass->PassHandle = renderGraph.CreatePass("GBuffer", passArgs);
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.GBufferAlbedo.OptimizedClearColor);
        commandList.ClearRenderTargetView(renderTargets[1], BuiltInRenderTargets.GBufferNormal.OptimizedClearColor);
        commandList.ClearRenderTargetView(renderTargets[2], BuiltInRenderTargets.GBufferSpecular.OptimizedClearColor);

        if (depthBuffer.HasValue)
        {
            commandList.ClearDepthStencilView(depthBuffer.AsPtr(), D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH, 1, 0, 0, null);
        }
    }
    [System]
    public static void CollectData(GBufferRenderPass* pass, ReadOnlySpan<Mesh3D> meshes)
    {
        //read all mesh data, should be a mem cpy
    }

    [System]
    public static void RecordCommandList(GBufferRenderPass* pass, in RenderGraph graph, in Window window)
    {
        if (!graph.Begin(pass->PassHandle, out var commandList))
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

        graph.End(pass->PassHandle);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(GBufferRenderPass* pass)
    {

    }
}
