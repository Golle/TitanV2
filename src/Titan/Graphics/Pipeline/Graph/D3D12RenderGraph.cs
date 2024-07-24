using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Strings;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Utils;
using Titan.Graphics.Rendering;
using Titan.Graphics.Resources;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Graphics.Pipeline.Graph;

[UnmanagedResource]
internal unsafe partial struct D3D12RenderGraph
{
    public bool IsReady => RenderPipelineReady;
    private Inline16<RenderPassGroup> Groups;
    private Inline10<D3D12RenderPass> Passes;
    private Inline16<D3D12RenderTarget> RenderTargets;
    private uint NumberOfGroups;
    private uint NumberOfPasses;
    private uint NumberOfRenderTargets;
    private bool RenderPipelineReady;
    private D3D12CommandQueue* CommandQueue;
    private D3D12Allocator* CommandAllocator;
    private D3D12ResourceManager* ResourceManager;
    private DXGISwapchain* Swapchain;

    [System(SystemStage.Init)]
    public static void Init(ref D3D12RenderGraph graph, in D3D12ResourceManager resourceManager, IConfigurationManager configurationManager, UnmanagedResourceRegistry registry, in Window window, in DXGISwapchain swapchain, in AssetsManager assetsManager)
    {
        using var _ = new MeasureTime<D3D12RenderGraph>("Constructed pipeline in {0} ms");
        var config = configurationManager.GetConfigOrDefault<RenderPipelineConfiguration>();
        Debug.Assert(config.PipelineConfigurationBuilder != null);
        var pipelineConfig = config.PipelineConfigurationBuilder();
        ValidatePipeline(pipelineConfig);

        (graph.NumberOfGroups, graph.NumberOfPasses, graph.NumberOfRenderTargets) = ConstructPipeline(graph.Groups, graph.Passes, graph.RenderTargets, pipelineConfig, resourceManager, window, swapchain, assetsManager);

        Logger.Info<D3D12RenderGraph>($"Pipeline constructed. Render Grouos = {graph.NumberOfGroups} Render Passes = {graph.NumberOfPasses} Render Targets = {graph.NumberOfRenderTargets}");
        graph.CommandQueue = registry.GetResourcePointer<D3D12CommandQueue>();
        graph.CommandAllocator = registry.GetResourcePointer<D3D12Allocator>();
        graph.ResourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
        graph.Swapchain = registry.GetResourcePointer<DXGISwapchain>();
    }


    [System(SystemStage.PreUpdate, SystemExecutionType.Inline)]
    public static void PreUpdate(ref D3D12RenderGraph graph, in AssetsManager assetsManager, in D3D12ResourceManager resourceManager)
    {
        if (graph.RenderPipelineReady)
        {
            return;
        }

        // check if any shader is still being loaded
        for (var i = 0; i < graph.NumberOfPasses; ++i)
        {
            var pass = graph.Passes.GetPointer(i);
            if (!assetsManager.IsLoaded(pass->Shader))
            {
                // still loading the assets
                return;
            }
        }

        // if all shaders have been loaded, read the shader data and get the PSOs.
        for (var i = 0; i < graph.NumberOfPasses; ++i)
        {
            var pass = graph.Passes.GetPointer(i);
            ref readonly var shader = ref assetsManager.Get(pass->Shader);
            pass->RootSignature = shader.RootSignature;
            pass->PipelineState = resourceManager.CreatePipelineState(new CreatePipelineStateArgs
            {
                RootSignature = pass->RootSignature,
                VertexShader = shader.VertexShader->ShaderByteCode,
                PixelShader = shader.PixelShader->ShaderByteCode,
                Depth = new()
                {
                    DepthEnabled = false
                },
                RenderTargets = pass->GetOutputs(),
                Topology = D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE
            });
        }
        // we call update cached passes when everything has been created
        graph.UpdateCachedPasses();
        graph.RenderPipelineReady = true;
    }

    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void PostUpdate(in D3D12RenderGraph graph, in D3D12CommandQueue commandQueue)
    {
        if (!graph.RenderPipelineReady)
        {
            return;
        }

        Span<CommandList> commandListBuffer = stackalloc CommandList[10];

        for (var i = 0; i < graph.NumberOfGroups; ++i)
        {
            ref readonly var group = ref graph.Groups[i];

            for (var j = 0; j < group.Count; ++j)
            {
                commandListBuffer[j] = graph.Passes[group.Offset + j].CommandList;
            }

            commandQueue.ExecuteCommandLists(commandListBuffer[..group.Count]);
        }
    }

    /// <summary>
    /// Request a render pass based on the identifier. This pass must have been marked as Custom.
    /// </summary>
    /// <param name="identifier">The unique identifier of the pass</param>
    /// <returns>The matching pass or Null</returns>
    public readonly RenderPass* GetRenderPass(string identifier)
    {
        for (var i = 0; i < NumberOfPasses; ++i)
        {
            if (Passes[i].Type == RenderPassType.Custom && Passes[i].Identifier.GetString() == identifier)
            {
                return (RenderPass*)Passes.GetPointer(i);
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
        for (var i = 0; i < NumberOfPasses; ++i)
        {
            if (Passes[i].Type == type)
            {
                return (RenderPass*)Passes.GetPointer(i);
            }
        }

        return null;
    }

    [SkipLocalsInit]
    public readonly CommandList BeginPass(RenderPass* renderPass)
    {
        Debug.Assert(renderPass != null);

        var pass = (D3D12RenderPass*)renderPass;
        ref var cachedState = ref pass->CachedResources;
        var pipelineState = (D3D12PipelineState*)ResourceManager->Access(pass->PipelineState);
        var commandList = CommandQueue->GetCommandList(pipelineState->Resource);

        //var handles = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[pass->OutputCount];
        //for (var i = 0; i < pass->OutputCount; ++i)
        //{
        //    handles[i] = ((D3D12Texture*)ResourceManager->Access(pass->Outputs[i]))->RTV.CPU;
        //    var texture = ResourceManager->Access(pass->Outputs[i]);

        //    if (Swapchain->CurrentBackbuffer == pass->Outputs[i])
        //    {
        //        commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);
        //    }
        //    else
        //    {
        //        commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);
        //    }
        //}
        commandList.ResourceBarriers(cachedState.BarriersBegin.AsPointer(), cachedState.BarriersBeginCount);

        //for (var i = 0; i < pass->InputCount; ++i)
        //{
        //    var texture = ResourceManager->Access(pass->Inputs[i]);
        //    commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
        //}

        commandList.SetRenderTargets(cachedState.Outputs.AsPointer(), pass->OutputCount);
        commandList.SetDescriptorHeap(CommandAllocator->SRV.Heap);

        //TODO(Jens): Transition resources

        return commandList;
    }

    public readonly void EndPass(RenderPass* renderPass, in CommandList commandList)
    {
        var pass = (D3D12RenderPass*)renderPass;
        ref var cachedState = ref pass->CachedResources;
        //for (var i = 0; i < pass->OutputCount; ++i)
        //{
        //    var texture = ResourceManager->Access(pass->Outputs[i]);
        //    if (Swapchain->CurrentBackbuffer == pass->Outputs[i])
        //    {
        //        commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        //    }
        //    else
        //    {
        //        commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON);
        //    }
        //}

        //for (var i = 0; i < pass->InputCount; ++i)
        //{
        //    var texture = ResourceManager->Access(pass->Inputs[i]);
        //    commandList.Transition(texture, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON);
        //}

        commandList.ResourceBarriers(cachedState.BarriersEnd.AsPointer(), cachedState.BarriersEndCount);
        //var group = Groups.GetPointer(renderPass->Group);
        commandList.Close();
        pass->CommandList = commandList;
    }


    private void UpdateCachedPasses()
    {
        for (var passIndex = 0; passIndex < NumberOfPasses; ++passIndex)
        {
            var pass = Passes.GetPointer(passIndex);

            pass->CachedResources.PipelineState = ((D3D12PipelineState*)ResourceManager->Access(pass->PipelineState))->Resource;
            pass->CachedResources.RootSignature = ((D3D12RootSignature*)ResourceManager->Access(pass->RootSignature))->Resource;

            TitanList<D3D12_RESOURCE_BARRIER> barriersBegin = pass->CachedResources.BarriersBegin;
            TitanList<D3D12_RESOURCE_BARRIER> barriersEnd = pass->CachedResources.BarriersEnd;

            // Go through all outputs
            for (var i = 0; i < pass->OutputCount; ++i)
            {
                var handle = pass->Outputs[i];
                var texture = (D3D12Texture*)ResourceManager->Access(handle);
                pass->CachedResources.Outputs[i] = texture->RTV.CPU;

                var transitionState = Swapchain->CurrentBackbuffer == handle
                    ? D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT
                    : D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;

                barriersBegin.Add(D3D12Helpers.Transition(texture->Resource, transitionState, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET));
                barriersEnd.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, transitionState));
            }

            //NOTE(Jens): This does not work since the resource for Backbuffer have to be replaced each frame.. yikes.
            // Go through all outputs
            for (var i = 0; i < pass->InputCount; ++i)
            {
                var handle = pass->Inputs[i];
                var texture = (D3D12Texture*)ResourceManager->Access(handle);
                pass->CachedResources.Inputs[i] = texture->RTV.CPU;

                barriersBegin.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE));
                barriersEnd.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON));
            }

            pass->CachedResources.BarriersBeginCount = (byte)barriersBegin.Count;
            pass->CachedResources.BarriersEndCount = (byte)barriersEnd.Count;
        }
    }

    //TODO(Jens): move this to another file
    private static (uint groups, uint passes, uint renderTargets) ConstructPipeline(
        Span<RenderPassGroup> groupsOut,
        Span<D3D12RenderPass> passesOut,
        Span<D3D12RenderTarget> renderTargetsOut,
        RenderPipeline config,
        in D3D12ResourceManager resourceManager,
        in Window window,
        in DXGISwapchain swapchain,
        AssetsManager assetsManager)
    {
        // The graph that will have the dependencies
        var graph = config.RenderPasses.ToDictionary(static c => c.Identifier, static _ => new List<string>());

        // Number of dependencies, used for sorting.
        var inDegree = config.RenderPasses.ToDictionary(static c => c.Identifier, _ => 0);

        // Go through all passes and map the dependencies by looking at the output from one pass and comparing to it the input of another pass.
        // If it's a match, add the dependant pass to the graph, and increaes the inDegree of the dependant pass
        foreach (var pass in config.RenderPasses)
        {
            foreach (var output in pass.Outputs)
            {
                foreach (var dependantPass in config.RenderPasses)
                {
                    if (dependantPass.Inputs.Any(i => i.Identifier == output.Identifier))
                    {
                        graph[pass.Identifier].Add(dependantPass.Identifier);
                        inDegree[dependantPass.Identifier]++;
                    }
                }
            }
        }
        // We use Kahn algorithm to sort the graph

        // Create a Queue and populate it with the "root" passes, these are passes that have no passes that depends on them
        var queue = new Queue<string>(inDegree.Where(static a => a.Value == 0).Select(static a => a.Key));

        TitanList<RenderPassGroup> groups = groupsOut;
        TitanList<D3D12RenderPass> passes = passesOut;
        TitanList<D3D12RenderTarget> renderTargets = renderTargetsOut;

        while (queue.Count > 0)
        {
            var passCount = queue.Count;
            var offset = passes.Count;
            var groupIndex = groups.Add(new RenderPassGroup((byte)offset, (byte)passCount));

            for (var i = 0; i < passCount; i++)
            {
                var identifier = queue.Dequeue();
                var pass = config.RenderPasses.First(pass => pass.Identifier == identifier);

                var renderPass = new D3D12RenderPass
                {
                    RenderPass =
                    {
                        Type = pass.Type
                    },
                    Identifier = StringRef.Create(pass.Identifier),
                    InputCount = (byte)pass.Inputs.Length,
                    OutputCount = (byte)pass.Outputs.Length,
                    Group = (byte)groupIndex,
                    Topology = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST,
                    Shader = assetsManager.Load<ShaderInfo>(pass.Shader),
                };

                if (!TryResolveTextures(renderPass.Outputs, pass.Outputs, ref renderTargets, resourceManager, window, swapchain))
                {
                    Logger.Error<D3D12RenderGraph>($"Failed to resolve the Output render targets.");
                }

                if (!TryResolveTextures(renderPass.Inputs, pass.Inputs, ref renderTargets, resourceManager, window, swapchain))
                {
                    Logger.Error<D3D12RenderGraph>($"Failed to resolve the Input render targets.");
                }

                foreach (var neighbor in graph[identifier])
                {
                    var count = --inDegree[neighbor];
                    if (count == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }

                passes.Add(renderPass);
            }

            //TODO(Jens): Add barriers here?
        }

        return (groups.Count, passes.Count, renderTargets.Count);


        static bool TryResolveTextures(Span<Handle<Texture>> handles, RenderPipelineRenderTarget[] targets, ref TitanList<D3D12RenderTarget> cache, D3D12ResourceManager resourceManager, in Window window, in DXGISwapchain swapchain)
        {
            for (var i = 0; i < targets.Length; ++i)
            {
                var target = targets[i];
                ref var handle = ref handles[i];
                if (target.Format is RenderTargetFormat.BackBuffer)
                {
                    handle = swapchain.CurrentBackbuffer;
                    continue;
                }

                // Try to get an already created texture.
                if (TryGetTexture(target.Identifier, out handle, cache.AsReadOnlySpan()))
                {
                    continue;
                }
                // no texture found with the identifier. Create a new one.
                var format = ToDXGIFormat(target.Format);
                handle = resourceManager.CreateTexture(new CreateTextureArgs
                {
                    Format = format,
                    Height = (uint)window.Height,
                    Width = (uint)window.Width,
                    RenderTargetView = true,
                    ShaderVisible = true
                });

                if (handle.IsInvalid)
                {
                    Logger.Error<D3D12RenderGraph>($"Failed to create the render target texture. Identifier = {target.Identifier} Format = {target.Format} DXGI_FORMAT = {format}");
                    return false;
                }
                cache.Add(new D3D12RenderTarget
                {
                    Identifier = StringRef.Create(target.Identifier),
                    Format = format,
                    Resource = handle,
                    X = 1.0f,
                    Y = 1.0f
                });
            }
            return true;

            static bool TryGetTexture(string identifier, out Handle<Texture> textureOut, ReadOnlySpan<D3D12RenderTarget> renderTargets)
            {
                Unsafe.SkipInit(out textureOut);
                foreach (ref readonly var renderTarget in renderTargets)
                {
                    if (renderTarget.Identifier.GetString() == identifier)
                    {
                        textureOut = renderTarget.Resource;
                        return true;
                    }
                }
                return false;
            }
        }
    }


    private static DXGI_FORMAT ToDXGIFormat(RenderTargetFormat format) => format switch
    {
        RenderTargetFormat.RGBA8 => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM,
        RenderTargetFormat.BackBuffer => DXGI_FORMAT.DXGI_FORMAT_UNKNOWN, // not sure how to handle this. 
        _ => DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM
    };


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
