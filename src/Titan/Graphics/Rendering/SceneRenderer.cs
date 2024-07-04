using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Components;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.Pipeline;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Graphics.Rendering;

[UnmanagedResource]
internal unsafe partial struct SceneRenderer
{
    private TitanList<Renderable> Renderables;
    private RenderPass* RenderPass;

    [System(SystemStage.Init)]
    public static void Init(ref SceneRenderer renderer, in D3D12RenderGraph renderGraph, IMemoryManager memoryManager)
    {
        var maxCount = 100_000u;
        if (!memoryManager.TryAllocList(out renderer.Renderables, maxCount))
        {
            Logger.Error<SceneRenderer>("Failed to allocate memory for renderables.");
            return;
        }
        renderer.RenderPass = renderGraph.GetRenderPass(RenderPassType.Scene);
        Debug.Assert(renderer.RenderPass != null);
    }

    [System(SystemStage.PreUpdate, SystemExecutionType.Inline)]
    public static void PreUpdate(ref SceneRenderer renderer)
    {
        renderer.Renderables.Clear();
    }

    /// <summary>
    /// The CollectData phase is an Entity System, this method can be called multiple times depending on the number of archetypes that have the Mesh and Transform data.
    /// <remarks>In the future this might be called from different threads.</remarks>
    /// </summary>
    [System]
    public static void CollectData(ref SceneRenderer renderer, ReadOnlySpan<Mesh3D> meshes, ReadOnlySpan<Transform3D> transforms)
    {
        // Do frustrum culling here? maybe this should just be a mem copy? the component should be the thing we publish to the GPU. 
        foreach (ref readonly var mesh in meshes)
        {
            renderer.Renderables.Add(new Renderable
            {
                Count = mesh.Count,
                AssetId = mesh.Asset.Index,
                Offset = mesh.Offset
            });
        }
    }

    [System]
    public static void Render(in SceneRenderer renderer, in D3D12RenderGraph graph, in RenderContext context, IAssetsManager assetsManager)
    {
        var renderPass = renderer.RenderPass;

        var commandList = graph.BeginPass(context, renderPass);
        var renderables = renderer.Renderables.AsReadOnlySpan();
        // copy mesh indices

        graph.EndPass(renderPass, commandList);
    }

}

[StructLayout(LayoutKind.Sequential)]
[SkipLocalsInit]
internal struct Renderable
{
    //NOTE(Jens): This should match what we access in the shader to read vertex data.
    public uint Offset;
    public uint Count;
    public int AssetId;
}

[UnmanagedResource]
public unsafe partial struct RenderContext
{
    internal Camera MainCamera;

    internal readonly D3D12CommandQueue* CommandQueue;
    internal readonly D3D12Allocator* Allocator;
    internal RenderContext(D3D12CommandQueue* queue, D3D12Allocator* allocator)
    {
        CommandQueue = queue;
        Allocator = allocator;
    }

    [System(SystemStage.Init, SystemExecutionType.Inline)]
    internal static void Init(RenderContext* context, UnmanagedResourceRegistry registry)
    {
        var allocator = registry.GetResourcePointer<D3D12Allocator>();
        var commandQueue = registry.GetResourcePointer<D3D12CommandQueue>();
        *context = new(commandQueue, allocator);
    }

}

internal struct Camera
{
    public Matrix4x4 Projection;
    public Matrix4x4 View;

}
