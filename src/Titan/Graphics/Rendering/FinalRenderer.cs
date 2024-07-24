using System.Diagnostics;
using Titan.Core.Maths;
using Titan.Graphics.Pipeline;
using Titan.Graphics.Pipeline.Graph;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.Rendering;

[UnmanagedResource]
internal unsafe partial struct FinalRenderer
{
    private RenderPass* Pass;

    [System(SystemStage.Init)]
    public static void Init(ref FinalRenderer renderer, in D3D12RenderGraph graph)
    {
        renderer.Pass = graph.GetRenderPass(RenderPassType.Backbuffer);
        Debug.Assert(renderer.Pass != null);
    }

    [System]
    public static void Update(in FinalRenderer renderer, in D3D12RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.IsReady)
        {
            return;
        }
        var commandList = graph.BeginPass(renderer.Pass);
        var t = ((D3D12RenderPass*)renderer.Pass)->Outputs[0];
        var color = Color.Green;
        commandList.ClearRenderTargetView(resourceManager.Access(t), &color);
        graph.EndPass(renderer.Pass, commandList);
    }
}
