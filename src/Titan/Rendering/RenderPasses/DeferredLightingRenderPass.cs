using Titan.Assets;
using Titan.Core;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32;
using Titan.Rendering.Storage;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;
using Titan.Core.Logging;
using static Titan.Assets.EngineAssetsRegistry.Shaders;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct DeferredLightingRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint RootConstantLightIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;
    private const uint RootConstantLightStorageIndex = RootConstantLightIndex + 1;

    [System(SystemStage.Init)]
    public static void Init(DeferredLightingRenderPass* renderPass, in RenderGraph graph)
    {
        renderPass->PassHandle = graph.CreatePass("DeferredLighting", new()
        {
            RootSignatureBuilder = static builder => builder
                .WithConstant(1, ShaderVisibility.Pixel)
                .WithDecriptorRange(1, register: 0, space: 0),
            BlendState = BlendStateType.Additive,
            Outputs = [BuiltInRenderTargets.DeferredLighting],
            Inputs =
            [
                BuiltInRenderTargets.GBufferPosition,
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular
            ],

            PixelShader = ShaderDeferredLightingPixel,
            VertexShader = ShaderDeferredLightingVertex,
            ClearFunction = &ClearFunction
        });
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.DeferredLighting.OptimizedClearColor);
    }

    [System(SystemStage.PreUpdate)]
    public static void BeginPass(in DeferredLightingRenderPass pass, in RenderGraph graph, in Window window, in LightStorage lightStorage, in D3D12ResourceManager resourceManager)
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

        D3D12_RECT rect = new()
        {
            Bottom = window.Height,
            Right = window.Width,
            Left = 0,
            Top = 0
        };

        commandList.SetViewport(&viewPort);
        commandList.SetScissorRect(&rect);

        commandList.SetGraphicsRootDescriptorTable(RootConstantLightStorageIndex, resourceManager.Access(lightStorage.LightStorageHandle)->SRV.GPU);

    }

    [System]
    public static void RenderLights(DeferredLightingRenderPass* pass, in RenderGraph graph, ReadOnlySpan<Light> lights)
    {
        if (!graph.IsReady)
        {
            return;
        }

        var commandList = graph.GetCommandList(pass->PassHandle);


        foreach (ref readonly var light in lights)
        {
            var index = (int)light.LightIndex;
            commandList.SetGraphicsRootConstant(RootConstantLightIndex, index);
            commandList.DrawInstanced(3, 1);
        }
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


    [System(SystemStage.Shutdown)]
    public static void Shutdown(DeferredLightingRenderPass* pass, in RenderGraph graph, in DXGISwapchain _) //NOTE(Jens): Get a Swapchain reference to make sure everything has been flushed before releasing it. A hack.. Need a better system for doing this.
    {
        Logger.Warning<DeferredLightingRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }
}
