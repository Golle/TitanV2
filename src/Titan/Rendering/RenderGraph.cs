using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Core.Strings;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering;

public record struct RenderTargetConfig(StringRef Name, RenderTargetFormat Format);

public record struct ShaderResourceConfig(string Name, RenderTargetFormat Format);
public record struct RenderTargetConfig1(string Name, RenderTargetFormat Format, bool Clear = false, Color ClearColor = default);
public ref struct CreateRenderPassArgs
{
    public required CreateRootSignatureArgs RootSignature;
    public ReadOnlySpan<RenderTargetConfig> Inputs;
    public ReadOnlySpan<RenderTargetConfig> Outputs;

    public ReadOnlySpan<ShaderResourceConfig> Inputs1;
    public ReadOnlySpan<RenderTargetConfig1> Outputs1;
    public AssetDescriptor VertexShader;
    public AssetDescriptor PixelShader;
    //public AssetDescriptor ComputerShader;

    /// <summary>
    /// A function pointer to a Clear function that will be called in Begin.
    /// 1. A span of Render Targets, same order as the pass was created.
    /// 2. An optional depth buffer
    /// 3. The command list
    /// </summary>
    public unsafe delegate*<ReadOnlySpan<Ptr<Texture>>, TitanOptional<Texture>, in CommandList, void> ClearFunction;
}

[UnmanagedResource]
internal unsafe partial struct RenderGraph
{
    private const byte HandleOffset = 123;

    private Inline16<RenderPass> _renderPasses;
    private Inline16<Ptr<RenderPass>> _sortedPasses;
    private Inline8<RenderPassGroup> _groups;

    private D3D12ResourceManager* _resourceManager;
    private D3D12CommandQueue* _commandQueue;
    private RenderTargetCache* _resourceTracker;

    private AssetsManager _assetsManager;
    private AtomicBumpAllocator _allocator;

    private bool IsReady;
    private int _renderPassCount;
    private uint _groupCount;

    private Handle<Texture> _backbufferHandle;


    [System(SystemStage.PreInit)]
    public static void PreInit(ref RenderGraph graph, IMemoryManager memoryManager, UnmanagedResourceRegistry registry, AssetsManager assetsManager)
    {
        if (!memoryManager.TryCreateAtomicBumpAllocator(out graph._allocator, MemoryUtils.KiloBytes(512)))
        {
            Logger.Error<RenderGraph>("Failed to create the bump allocator.");
            return;
        }

        graph._resourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
        graph._commandQueue = registry.GetResourcePointer<D3D12CommandQueue>();
        graph._resourceTracker = registry.GetResourcePointer<RenderTargetCache>();
        graph._assetsManager = assetsManager;
        graph._renderPassCount = 0;
        graph._sortedPasses = default;
        graph._groups = default;
    }


    public readonly Handle<RenderPass> CreatePass(string name, in CreateRenderPassArgs args)
    {
        var index = Interlocked.Increment(ref Unsafe.AsRef(in _renderPassCount)) - 1;
        Debug.Assert(index < _renderPasses.Size, $"To many render passes created. Change the Inline array. Max = {_renderPasses.Size}");

        var pass = _renderPasses.AsPointer() + index;
        pass->Name = StringRef.Create(name);

        pass->RootSignature = _resourceManager->CreateRootSignature(args.RootSignature);
        //NOTE(Jens): Need support for Compute shaders as well.
        pass->PixelShader = _assetsManager.Load<ShaderAsset>(args.PixelShader);
        pass->VertexShader = _assetsManager.Load<ShaderAsset>(args.VertexShader);

        pass->Outputs = _allocator.AllocateArray<Handle<Texture>>(args.Outputs.Length);
        pass->Inputs = _allocator.AllocateArray<Handle<Texture>>(args.Inputs.Length);

        for (var i = 0; i < args.Outputs.Length; ++i)
        {
            pass->Outputs[i] = _resourceTracker->GetOrCreateRenderTarget(args.Outputs[i]);
        }

        for (var i = 0; i < args.Inputs.Length; ++i)
        {
            pass->Inputs[i] = _resourceTracker->GetOrCreateRenderTarget(args.Inputs[i]);
        }

        pass->ClearFunction = args.ClearFunction != null ? args.ClearFunction : &ClearFunctionStub;

        return index + HandleOffset;
    }

    public readonly bool Begin(in Handle<RenderPass> handle, out CommandList commandList)
    {
        Unsafe.SkipInit(out commandList);
        if (!IsReady)
        {
            return false;
        }

        var index = handle.Value - HandleOffset;
        var pass = _renderPasses.AsPointer() + index;

        pass->CommandList = commandList = _commandQueue->GetCommandList(pass->PipelineState);
        pass->CommandList.SetGraphicsRootSignature(_resourceManager->Access(pass->RootSignature)->Resource);
        pass->CommandList.SetTopology(D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

        //NOTE(Jens): A lot of stack allocs, do we need all of them? :O
        TitanList<D3D12_RESOURCE_BARRIER> barriers = stackalloc D3D12_RESOURCE_BARRIER[10];
        TitanList<D3D12_CPU_DESCRIPTOR_HANDLE> renderTargets = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[(int)pass->Outputs.Length];
        TitanList<Ptr<Texture>> renderTargetTextures = stackalloc Ptr<Texture>[(int)pass->Outputs.Length];
        TitanList<int> inputTextures = stackalloc int[(int)pass->Inputs.Length];

        foreach (ref readonly var output in pass->Outputs.AsReadOnlySpan())
        {
            var isBackbuffer = output == _backbufferHandle;
            var transitionState = isBackbuffer
                ? D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT
                : D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;

            var texture = _resourceManager->Access(output);
            renderTargetTextures.Add(texture);
            barriers.Add(D3D12Helpers.Transition(texture->Resource, transitionState, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET));
            renderTargets.Add(texture->RTV.CPU);
        }

        foreach (ref readonly var input in pass->Inputs.AsReadOnlySpan())
        {
            var texture = _resourceManager->Access(input);
            //TODO(Jens): This transition should consider if it's a Pixel resource only or not.
            barriers.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_ALL_SHADER_RESOURCE));
            inputTextures.Add(texture->SRV.Index);
        }
        //TODO(Jens): Add InputTextures to the Root Constants. 

        commandList.SetRenderTargets(renderTargets.AsPointer(), renderTargets.Count);
        commandList.ResourceBarriers(barriers);
        pass->ClearFunction(renderTargetTextures, null, commandList);

        return true;
    }

    public readonly void End(in Handle<RenderPass> handle)
    {
        var index = handle.Value - HandleOffset;
        var pass = _renderPasses.AsPointer() + index;
        TitanList<D3D12_RESOURCE_BARRIER> barriers = stackalloc D3D12_RESOURCE_BARRIER[10];
        foreach (var output in pass->Outputs.AsReadOnlySpan())
        {
            var isBackbuffer = output == _backbufferHandle;
            var transitionState = isBackbuffer
                ? D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT
                : D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON;

            var texture = _resourceManager->Access(output);
            barriers.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, transitionState));
        }

        foreach (var input in pass->Inputs.AsReadOnlySpan())
        {
            var texture = _resourceManager->Access(input);
            barriers.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_ALL_SHADER_RESOURCE, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON));
        }
        pass->CommandList.ResourceBarriers(barriers);
        pass->CommandList.Close();
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(ref RenderGraph graph, in D3D12ResourceManager resourceManager, in DXGISwapchain swapchain)
    {
        if (graph.IsReady)
        {
            return;
        }

        if (!graph.AreAssetsLoaded())
        {
            return;
        }

        if (!graph.CreatePipelineStates())
        {
            Logger.Error<RenderGraph>("Failed to create the pipeline states. this is FATAL, no recovering..");
            return;
        }
        graph.SortRenderGraph();

        graph.IsReady = true;
        graph._backbufferHandle = swapchain.CurrentBackbuffer;
    }


    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void PostUpdate(in RenderGraph graph, in D3D12CommandQueue commandQueue)
    {
        if (!graph.IsReady)
        {
            return;
        }

        Span<CommandList> commandListBuffer = stackalloc CommandList[10];

        for (var groupIndex = 0; groupIndex < graph._groupCount; ++groupIndex)
        {
            ref readonly var group = ref graph._groups[groupIndex];

            for (var i = 0; i < group.Count; ++i)
            {
                commandListBuffer[i] = graph._sortedPasses[group.Offset + i].Get()->CommandList;
            }

            commandQueue.ExecuteCommandLists(commandListBuffer[..group.Count]);
        }
    }


    private bool AreAssetsLoaded()
    {
        foreach (ref readonly var pass in _renderPasses.AsReadOnlySpan()[.._renderPassCount])
        {
            if (pass.VertexShader.IsValid && !_assetsManager.IsLoaded(pass.VertexShader))
            {
                return false;
            }

            if (pass.PixelShader.IsValid && !_assetsManager.IsLoaded(pass.PixelShader))
            {
                return false;
            }
        }

        return true;
    }

    private bool CreatePipelineStates()
    {
        foreach (ref var pass in _renderPasses.AsSpan()[.._renderPassCount])
        {
            pass.PipelineState = _resourceManager->CreatePipelineState(new CreatePipelineStateArgs
            {
                RenderTargets = pass.Outputs,
                RootSignature = pass.RootSignature,
                VertexShader = _assetsManager.Get(pass.VertexShader).ShaderByteCode,
                PixelShader = _assetsManager.Get(pass.PixelShader).ShaderByteCode,
                Topology = D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE
            });
            if (pass.PipelineState.IsInvalid)
            {
                Logger.Error<RenderGraph>($"Failed to create the pipeline state for render pass. Name = {pass.Name.GetString()}");
                return false;
            }
        }

        return true;
    }

    private void SortRenderGraph()
    {
        Span<GraphDependencies> graph = stackalloc GraphDependencies[_renderPassCount];

        // Go through all passes and map the dependencies by looking at the output from one pass and comparing to it the input of another pass.
        // If it's a match, add the dependant pass to the graph, and increaes the inDegree of the dependant pass
        for (var outer = 0; outer < _renderPassCount; ++outer)
        {
            foreach (var output in _renderPasses[outer].Outputs.AsReadOnlySpan())
            {
                for (var inner = 0; inner < _renderPassCount; ++inner)
                {
                    if (inner == outer)
                    {
                        continue;
                    }

                    if (Contains(_renderPasses[inner].Inputs, output))
                    {
                        // this is a dependency
                        graph[outer].AddDependency(inner);
                        graph[inner].InDegree++;
                    }

                }
            }
        }

        // Create a queue and put the passes without dependencies there (in degree == 0)
        TitanQueue<int> queue = stackalloc int[128];
        for (var i = 0; i < _renderPassCount; ++i)
        {
            if (graph[i].InDegree == 0)
            {
                queue.Enqueue(i);
            }
        }

        Debug.Assert(!queue.IsEmpty());

        //TODO(Jens): Detect circular dependencies
        // Go through all the passes and create render groups based on it's dependencies.
        TitanList<Ptr<RenderPass>> sortedPasses = _sortedPasses;
        TitanList<RenderPassGroup> groups = _groups;
        while (queue.HasItems())
        {
            var passCount = queue.Count;
            var offset = sortedPasses.Count;

            groups.Add(new RenderPassGroup((byte)offset, (byte)passCount));
            for (var i = 0; i < passCount; ++i)
            {
                var passIndex = queue.Dequeue();
                foreach (var neighbor in graph[passIndex].GetDependencies())
                {
                    var count = --graph[neighbor].InDegree;
                    if (count == 0)
                    {
                        queue.Enqueue(neighbor);
                    }
                }

                sortedPasses.Add(_renderPasses.GetPointer(passIndex));
            }
        }

        _groupCount = groups.Count;


        static bool Contains(ReadOnlySpan<Handle<Texture>> textures, in Handle<Texture> handle)
        {
            foreach (var texture in textures)
            {
                if (handle == texture)
                {
                    return true;
                }
            }
            return false;
        }
    }

    private struct GraphDependencies
    {
        public uint InDegree;
        private Inline8<int> _dependsOn;
        private int _dependencyCount;
        public void AddDependency(int index) => _dependsOn[_dependencyCount++] = index;
        public ReadOnlySpan<int> GetDependencies() => _dependsOn.AsReadOnlySpan()[.._dependencyCount];
    }


    private static void ClearFunctionStub(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        // noop
    }
}