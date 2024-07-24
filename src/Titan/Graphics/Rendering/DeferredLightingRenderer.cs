using System.Diagnostics;
using Titan.Graphics.Pipeline;
using Titan.Graphics.Pipeline.Graph;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.Rendering;

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
