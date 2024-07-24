using System.Diagnostics;
using Titan.Application;
using Titan.Core.Maths;
using Titan.Graphics.Pipeline;
using Titan.Graphics.Pipeline.Graph;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.Rendering.D3D12;
internal class D3D12RenderingModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddSystemsAndResource<D3D12FullScreenRenderer>()
            //.AddSystemsAndResource<D3D12TextRenderer>()

            .AddSystemsAndResource<SceneRenderer>()
            .AddSystemsAndResource<DeferredLightingRenderer>()
            .AddSystemsAndResource<FinalRenderer>()
            
            ;

        return true;
    }
}

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
