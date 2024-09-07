using Titan.Assets;
using Titan.Core;
using Titan.ECS.Components;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct DeferredLightingRenderPass
{
    private Handle<RenderPass> PassHandle;

    [System(SystemStage.Init)]
    public static void Init(DeferredLightingRenderPass* renderPass, in RenderGraph graph)
    {
        renderPass->PassHandle = graph.CreatePass("DeferredLighting", new()
        {
            Outputs = [BuiltInRenderTargets.DeferredLighting],
            Inputs =
            [
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular
            ],
            PixelShader = EngineAssetsRegistry.ShaderDeferredLightingPixel,
            VertexShader = EngineAssetsRegistry.ShaderDeferredLightingVertex,
            ClearFunction = &ClearFunction
        });
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.DeferredLighting.OptimizedClearColor);
    }

    [System(SystemStage.PreUpdate)]
    public static void BeginPass(in DeferredLightingRenderPass pass, in RenderGraph graph, in Window window)
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
    }

    [System]
    public static void RenderLights(DeferredLightingRenderPass* pass, in RenderGraph graph, ReadOnlySpan<Light> lights, ReadOnlySpan<Transform3D> transforms)
    {

        if (!graph.IsReady)
        {
            return;
        }

        var commandList = graph.GetCommandList(pass->PassHandle);

        


    }

    [System]
    public static void EndPass(in DeferredLightingRenderPass pass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.IsReady)
        {
            return;
        }
        var commandList = graph.GetCommandList(pass.PassHandle);

        commandList.DrawInstanced(3, 1);

        graph.End(pass.PassHandle);
    }
}
