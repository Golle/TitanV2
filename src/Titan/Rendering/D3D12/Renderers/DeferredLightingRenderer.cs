using System.Diagnostics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.D3D12.Pipeline;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.D3D12.Renderers;

[UnmanagedResource]
internal unsafe partial struct DeferredLightingRenderer
{
    private RenderPass* Pass;

    [System(SystemStage.Init)]
    public static void Init(ref DeferredLightingRenderer renderer, in D3D12RenderGraph graph)
    {
        renderer.Pass = graph.GetRenderPass(RenderPassType.DeferredLighting); 
        Debug.Assert(renderer.Pass != null);
    }

    [System]
    public static void Update(in DeferredLightingRenderer renderer, in D3D12RenderGraph graph, in D3D12ResourceManager resourceManager, in Window window)
    {
        if (!graph.IsReady)
        {
            return;
        }
        var commandList = graph.BeginPass(renderer.Pass);

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
        
        graph.EndPass(renderer.Pass, commandList);
    }
}
