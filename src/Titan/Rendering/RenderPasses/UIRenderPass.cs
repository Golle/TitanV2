using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Resources;
using Titan.Systems;
using Titan.UI;
using Titan.UI2;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct UIRenderPass
{
    private Handle<RenderPass> PassHandle;
    private Handle<GPUBuffer> IndexBuffer;
    private const uint PassDataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    [System(SystemStage.Init)]
    public static void Init(UIRenderPass* renderPass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        TitanArray<ushort> indexBuffer = stackalloc ushort[6];
        D3D12Helpers.InitSquareIndexBuffer(indexBuffer);
        renderPass->IndexBuffer = resourceManager.CreateBuffer(CreateBufferArgs.Create<ushort>(indexBuffer.Length, BufferType.Index, indexBuffer.AsBuffer()));
        if (renderPass->IndexBuffer.IsInvalid)
        {
            Logger.Error<UIRenderPass>("Failed to create the IndexBuffer.");
            return;
        }

        var args = new CreateRenderPassArgs
        {
            BlendState = BlendStateType.AlphaBlend,
            CullMode = CullMode.Back,
            ClearFunction = &Clear,
            DepthBuffer = null,
            Inputs = [],
            Outputs = [BuiltInRenderTargets.UI],
            PixelShader = EngineAssetsRegistry.Shaders.ShaderUIPixel,
            VertexShader = EngineAssetsRegistry.Shaders.ShaderUIVertex,
            RootSignatureBuilder = builder => builder
                .WithDecriptorRange(1, space: 0) // UI Elements 
        };


        renderPass->PassHandle = graph.CreatePass("UI", args);
    }


    [System]
    public static void Update(in UIRenderPass pass, in RenderGraph graph, in Window window, in D3D12ResourceManager resourceManager, in UISystem system, in UISystem2 system2)
    {
        if (!graph.Begin(pass.PassHandle, out var commandList))
        {
            return;
        }

        //NOTE(Jens): We can cache these in the UI system.
        var elementsIndex = resourceManager.Access(system2.GetCurrentGPUHandle())->SRV.GPU;
        //var glyphsIndex = resourceManager.Access(system.GlyphInstances)->SRV.GPU;
        var indexBuffer = resourceManager.Access(pass.IndexBuffer);

        commandList.SetGraphicsRootDescriptorTable(PassDataIndex, elementsIndex);
        //commandList.SetGraphicsRootDescriptorTable(PassDataIndex + 1, glyphsIndex);
        commandList.SetIndexBuffer(indexBuffer);
    }

    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void PostUpdate(in UIRenderPass pass, in UISystem ui, in UISystem2 ui2, ref RenderGraph graph)
    {
        if (!graph.IsReady)
        {
            return;
        }

        //NOTE(Jens): We take RenderGraph as a mutable reference so it executes before the command lists are executed. 
        //NOTE(Jens): Maybe we need to rethink the way this is executed. 
        var commandList = graph.GetCommandList(pass.PassHandle);

        commandList.DrawIndexedInstanced(6, ui2.GetCount());
        //if (ui is { Count: > 0, Visible: true })
        //{
        //    commandList.DrawIndexedInstanced(6, ui.Count);
        //}

        graph.End(pass.PassHandle);
    }

    private static void Clear(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
        => commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.UI.OptimizedClearColor);

    [System(SystemStage.Shutdown)]
    public static void Shutdown(UIRenderPass* pass, in RenderGraph graph, in DXGISwapchain _) //NOTE(Jens): Get a Swapchain reference to make sure everything has been flushed before releasing it. A hack.. Need a better system for doing this.
    {
        Logger.Warning<UIRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }
}


