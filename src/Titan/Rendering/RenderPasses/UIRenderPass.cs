using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Graphics;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct UIRenderPass
{
    private Handle<RenderPass> PassHandle;

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
        };

        renderPass->PassHandle = graph.CreatePass("UI", args);
    }


    [System]
    public static void Update(UIRenderPass* pass, in RenderGraph graph, in Window window)
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

        D3D12_RECT rect = new()
        {
            Bottom = window.Height,
            Right = window.Width,
            Left = 0,
            Top = 0
        };
        commandList.SetScissorRect(&rect);
        commandList.SetViewport(&viewPort);

        commandList.DrawInstanced(3, 1);

        graph.End(pass->PassHandle);
    }


    private static void Clear(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList) 
        => commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.UI.OptimizedClearColor);
}


