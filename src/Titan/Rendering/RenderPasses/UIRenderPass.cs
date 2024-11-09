using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.UI;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct UIRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint PassDataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    [System(SystemStage.Init)]
    public static void Init(UIRenderPass* renderPass, in RenderGraph graph)
    {
        Logger.Error<UIRenderPass>("init UI render pass");

        var args = new CreateRenderPassArgs
        {
            BlendState = BlendStateType.AlphaBlend,
            ClearFunction = &Clear,
            DepthBuffer = null,
            Inputs = [],
            Outputs = [BuiltInRenderTargets.UI],
            PixelShader = EngineAssetsRegistry.ShaderUIPixel,
            VertexShader = EngineAssetsRegistry.ShaderUIVertex,
            RootSignatureBuilder = builder => builder
                .WithDecriptorRange(1, space: 0) // Renderable instances
        };

        renderPass->PassHandle = graph.CreatePass("UI", args);
    }


    [System]
    public static void Update(in UIRenderPass pass, in RenderGraph graph, in Window window, in D3D12ResourceManager resourceManager, in UISystem system)
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
        commandList.SetScissorRect(&rect);
        commandList.SetViewport(&viewPort);

        var instancesIndex = resourceManager.Access(system.Instances)->SRV.GPU;
        commandList.SetGraphicsRootDescriptorTable(PassDataIndex, instancesIndex);
    }

    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void PostUpdate(in UIRenderPass pass, in UISystem ui, ref RenderGraph graph)
    {
        if (!graph.IsReady)
        {
            return;
        }

        //NOTE(Jens): We take RenderGraph as a mutable reference so it executes before the command lists are executed. 
        //NOTE(Jens): Maybe we need to rethink the way this is executed. 
        var commandList = graph.GetCommandList(pass.PassHandle);

        if (ui.Count > 0)
        {
            //TODO(Jens): replace this with DrawIndexedInstanced and use an index buffer
            commandList.DrawInstanced(6, ui.Count);
        }

        graph.End(pass.PassHandle);
    }

    private static void Clear(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
        => commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.UI.OptimizedClearColor);
}


