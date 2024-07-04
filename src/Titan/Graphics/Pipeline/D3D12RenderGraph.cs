using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Memory;
using Titan.Core.Strings;
using Titan.Graphics.D3D12;
using Titan.Graphics.Rendering;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Graphics.Pipeline;

internal unsafe struct D3D12RenderPass
{
    public RenderPass RenderPass;
    public StringRef Identifier;
    public Inline4<Handle<Texture>> Inputs;
    public Inline4<Handle<Texture>> Outputs;
    public Handle<Texture> DepthBufferInput;
    public Handle<Texture> DepthBufferOutput;

    public byte InputCount;
    public byte OutputCount;

    public D3D_PRIMITIVE_TOPOLOGY Topology;
    public ComPtr<ID3D12PipelineState> DefaultPipelineState;
    public ComPtr<ID3D12RootSignature> RootSignature;
    public RenderPassType Type => RenderPass.Type;
}

public struct RenderPass
{
    public RenderPassType Type;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RenderPassGroup
{
    public byte Offset;
    public byte Count;
}

[UnmanagedResource]
internal unsafe partial struct D3D12RenderGraph
{
    private Inline16<RenderPassGroup> _groups;
    private Inline10<D3D12RenderPass> _passes;
    private uint _numberOfGroups;
    private uint _numberOfPasses;

    private D3D12CommandQueue* _commandQueue;

    [System(SystemStage.Init)]
    public static void Init(ref D3D12RenderGraph graph, in D3D12ResourceManager resourceManager, in Window window, IConfigurationManager configurationManager, IMemoryManager memoryManager, UnmanagedResourceRegistry registry)
    {
        var config = configurationManager.GetConfigOrDefault<RenderPipelineConfiguration>();
        Debug.Assert(config.PipelineConfigurationBuilder != null);
        var pipelineConfig = config.PipelineConfigurationBuilder();
        ValidatePipeline(pipelineConfig);

        var groups = RenderGraphBuilder.Build(graph._passes, graph._groups, pipelineConfig.RenderPasses);
        graph._numberOfGroups = (uint)groups;
        graph._numberOfPasses = (uint)pipelineConfig.RenderPasses.Length;
        graph._commandQueue = registry.GetResourcePointer<D3D12CommandQueue>();
    }

    /// <summary>
    /// Request a render pass based on the identifier. This pass must have been marked as Custom.
    /// </summary>
    /// <param name="identifier">The unique identifier of the pass</param>
    /// <returns>The matching pass or Null</returns>
    public readonly RenderPass* GetRenderPass(string identifier)
    {
        for (var i = 0; i < _numberOfPasses; ++i)
        {
            if (_passes[i].Type == RenderPassType.Custom && _passes[i].Identifier.GetString() == identifier)
            {
                return (RenderPass*)_passes.GetPointer(i);
            }
        }

        return null;
    }

    /// <summary>
    /// Request the Render pass by type. These are guaranteed to exist.
    /// </summary>
    /// <param name="type">Any type except Custom.</param>
    /// <returns>The pass matching the type</returns>
    public readonly RenderPass* GetRenderPass(RenderPassType type)
    {
        Debug.Assert(type != RenderPassType.Custom);
        for (var i = 0; i < _numberOfPasses; ++i)
        {
            if (_passes[i].Type == type)
            {
                return (RenderPass*)_passes.GetPointer(i);
            }
        }

        return null;
    }

    [SkipLocalsInit]
    public readonly CommandList BeginPass(in RenderContext context, RenderPass* renderPass)
    {
        Debug.Assert(renderPass != null);

        var pass = (D3D12RenderPass*)renderPass;
        var commandList = context.CommandQueue->GetCommandList(pass->DefaultPipelineState);
        //commandList.SetRenderTargets(pass->OutputHandles.GetPointer(0), pass->OutputCount, &pass->DepthBufferHandle);
        commandList.SetDescriptorHeap(context.Allocator->SRV.Heap);


        // transition resources ?

        return commandList;
    }

    public readonly void EndPass(RenderPass* pass, CommandList commandList)
    {


    }

    [Conditional("DEBUG")]
    private static void ValidatePipeline(RenderPipeline pipelineConfig)
    {
        ValidateHasOne(pipelineConfig.RenderPasses, RenderPassType.Scene);
        ValidateHasOne(pipelineConfig.RenderPasses, RenderPassType.Backbuffer);
        ValidateHasOne(pipelineConfig.RenderPasses, RenderPassType.DeferredLighting);
        static void ValidateHasOne(RenderPipelinePass[] passes, RenderPassType type)
        {
            var count = passes.Count(p => p.Type == type);
            Debug.Assert(count == 1, $"The render pipeline configuration must have One {type} pass.");
        }
    }
}
