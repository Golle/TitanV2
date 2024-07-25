using System.Diagnostics;
using Titan.Rendering.D3D12.Pipeline;
using Titan.Resources;
using Titan.Systems;
using D3D12ResourceManager = Titan.Graphics.D3D12.D3D12ResourceManager;

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
    public static void Update(in DeferredLightingRenderer renderer, in D3D12RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.IsReady)
        {
            return;
        }
        var commandList = graph.BeginPass(renderer.Pass);
        
        graph.EndPass(renderer.Pass, commandList);
    }
}
