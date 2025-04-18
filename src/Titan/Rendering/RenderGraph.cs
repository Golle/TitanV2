using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Application;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Core.Strings;
using Titan.ECS.Systems;
using Titan.Events;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Utils;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 256)]
internal struct FrameData
{
    public Matrix4x4 WorldMatrix;
    public Matrix4x4 ViewProjection;
    public Vector3 CameraPosition;
    public uint WindowWidth;
    public uint WindowHeight;
    public float Time;
}

public enum DepthBufferMode : byte
{
    Write,
    Read
}

public record struct RenderTargetConfig(StringRef Name, RenderTargetFormat Format, Color OptimizedClearColor = default, float ClearValue = 1f);
public record struct DepthBufferConfig(StringRef Name, DepthBufferFormat Format, float ClearValue = 1f, bool ShaderVisible = false, int Width = -1, int Height = -1);


public readonly record struct ShaderGroup
{
    public AssetDescriptor? VertexShader { get; init; }
    public AssetDescriptor? PixelShader { get; init; }
}

public ref struct CreateRenderPassArgs
{
    public Func<RootSignatureBuilder, RootSignatureBuilder>? RootSignatureBuilder;
    public ReadOnlySpan<RenderTargetConfig> Inputs;
    public ReadOnlySpan<RenderTargetConfig> Outputs;
    public DepthBufferConfig? DepthBuffer;
    public DepthBufferMode DepthBufferMode;


    [Obsolete]
    public AssetDescriptor VertexShader;
    [Obsolete]
    public AssetDescriptor? PixelShader;

    public ReadOnlySpan<ShaderGroup> Shaders;


    public Viewport? Viewport;
    public Rect? ScissorRect;

    public BlendStateType BlendState;
    public CullMode CullMode;
    public FillMode FillMode;
    public PrimitiveTopology Topology;
    /// <summary>
    /// The order to sort this render pass, only works for passes that has the same outputs.
    /// <remarks>Higher number means LATER, so set it to a negative number if it should run before other passes.</remarks>
    /// </summary>
    public sbyte Order;
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
public unsafe partial struct RenderGraph
{
    //NOTE(Jens): Make sure these are updated when we implement more.
    public enum RootSignatureIndex : uint
    {
        Texture2D = 0,
        InputTexturesIndex = 1,
        FrameDataIndex = 2,
        CustomIndexStart
    }

    private const byte HandleOffset = 123;

    private Inline16<RenderPass> _renderPasses;
    private Inline16<Ptr<RenderPass>> _sortedPasses;
    private Inline8<RenderPassGroup> _groups;

    private D3D12ResourceManager* _resourceManager;
    private D3D12CommandQueue* _commandQueue;
    private D3D12Allocator* _d3d12Allocator;
    private RenderTargetCache* _resourceTracker;

    private AssetsManager _assetsManager;
    private AtomicBumpAllocator _allocator;

    private bool _isReady;
    private int _renderPassCount;
    private uint _groupCount;
    public readonly bool IsReady => _isReady;

    private Handle<Texture> _backbufferHandle;
    private Handle<GPUBuffer> _frameDataConstantBuffer;
    private MappedGPUResource<FrameData> _frameDataGPU;


    [System(SystemStage.PreInit)]
    internal static void PreInit(ref RenderGraph graph, IMemoryManager memoryManager, UnmanagedResourceRegistry registry, AssetsManager assetsManager)
    {
        if (!memoryManager.TryCreateAtomicBumpAllocator(out graph._allocator, MemoryUtils.KiloBytes(512)))
        {
            Logger.Error<RenderGraph>("Failed to create the bump allocator.");
            return;
        }

        graph._resourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
        graph._commandQueue = registry.GetResourcePointer<D3D12CommandQueue>();
        graph._resourceTracker = registry.GetResourcePointer<RenderTargetCache>();
        graph._d3d12Allocator = registry.GetResourcePointer<D3D12Allocator>();
        graph._assetsManager = assetsManager;
        graph._renderPassCount = 0;
        graph._sortedPasses = default;
        graph._groups = default;
    }

    [System(SystemStage.Init)]
    public static void Init(RenderGraph* graph, in D3D12ResourceManager resourceManager)
    {
        graph->_frameDataConstantBuffer = resourceManager.CreateBuffer(CreateBufferArgs.Create<FrameData>(1, BufferType.Constant, cpuVisible: true, shaderVisible: true));
        if (graph->_frameDataConstantBuffer.IsInvalid)
        {
            Logger.Error<RenderGraph>("Failed to allocate the frame data buffer.");
            return;
        }

        if (!resourceManager.TryMapBuffer(graph->_frameDataConstantBuffer, out graph->_frameDataGPU))
        {
            Logger.Error<RenderGraph>("Failed to map the frame data constant buffer.");
            return;
        }
    }

    public readonly Handle<RenderPass> CreatePass(string name, in CreateRenderPassArgs args)
    {
        var index = Interlocked.Increment(ref Unsafe.AsRef(in _renderPassCount)) - 1;
        Debug.Assert(index < _renderPasses.Size, $"To many render passes created. Change the Inline array. Max = {_renderPasses.Size}");

        var pass = _renderPasses.AsPointer() + index;
        pass->Name = StringRef.Create(name);


        var numberOfSRVs = args.Inputs.Length;
        if (args.DepthBuffer != null && args.DepthBufferMode == DepthBufferMode.Read)
        {
            // Depth buffer reads are a bit different than normal inputs.
            numberOfSRVs++;
        }

        var builder = CreateDefaultRootSignatureBuilder((byte)numberOfSRVs);
        var rootSignatureArgs = (args.RootSignatureBuilder != null
                ? args.RootSignatureBuilder(builder)
                : builder)
            .Build();

        pass->RootSignature = _resourceManager->CreateRootSignature(rootSignatureArgs);
        //NOTE(Jens): Need support for Compute shaders as well.

        if (args.Shaders.Length == 0)
        {
            Logger.Warning<RenderGraph>($"Render pass {name} is using old shader structure.");
            pass->VertexShaders = _allocator.AllocateArray<AssetHandle<ShaderAsset>>(1);
            pass->PixelShaders = _allocator.AllocateArray<AssetHandle<ShaderAsset>>(1);
            pass->PipelineStates = _allocator.AllocateArray<Handle<PipelineState>>(1);
            pass->PixelShaders[0] = args.PixelShader.HasValue ? _assetsManager.Load<ShaderAsset>(args.PixelShader.Value) : AssetHandle<ShaderAsset>.Invalid;
            pass->VertexShaders[0] = _assetsManager.Load<ShaderAsset>(args.VertexShader);

#if HOT_RELOAD_ASSETS
            pass->ShaderHashes = _allocator.AllocateArray<ulong>(1);
#endif
        }
        else
        {
            pass->VertexShaders = _allocator.AllocateArray<AssetHandle<ShaderAsset>>(args.Shaders.Length);
            pass->PixelShaders = _allocator.AllocateArray<AssetHandle<ShaderAsset>>(args.Shaders.Length);
            pass->PipelineStates = _allocator.AllocateArray<Handle<PipelineState>>(args.Shaders.Length);

            for (var i = 0; i < args.Shaders.Length; ++i)
            {
                ref readonly var shader = ref args.Shaders[i];
                pass->VertexShaders[i] = shader.VertexShader.HasValue ? _assetsManager.Load<ShaderAsset>(shader.VertexShader.Value) : AssetHandle<ShaderAsset>.Invalid;
                pass->PixelShaders[i] = shader.PixelShader.HasValue ? _assetsManager.Load<ShaderAsset>(shader.PixelShader.Value) : AssetHandle<ShaderAsset>.Invalid;
            }

#if HOT_RELOAD_ASSETS
            pass->ShaderHashes = _allocator.AllocateArray<ulong>(args.Shaders.Length);
#endif
        }

        pass->Outputs = _allocator.AllocateArray<Handle<Texture>>(args.Outputs.Length);
        pass->Inputs = _allocator.AllocateArray<Handle<Texture>>(args.Inputs.Length);
        pass->BlendState = args.BlendState;
        pass->CullMode = args.CullMode;
        pass->FillMode = args.FillMode;
        pass->Topology = args.Topology;

        pass->Viewport = args.Viewport ?? default;
        pass->ScissorRect = args.ScissorRect ?? default;
        pass->UseDefaultViewport = !args.Viewport.HasValue;

        pass->DepthBufferMode = args.DepthBufferMode;
        pass->Order = args.Order;
        for (var i = 0; i < args.Outputs.Length; ++i)
        {
            pass->Outputs[i] = _resourceTracker->GetOrCreateRenderTarget(args.Outputs[i]);
        }

        for (var i = 0; i < args.Inputs.Length; ++i)
        {
            pass->Inputs[i] = _resourceTracker->GetOrCreateRenderTarget(args.Inputs[i]);
        }

        if (args.DepthBuffer.HasValue)
        {
            pass->DepthBuffer = _resourceTracker->GetOrCreateDepthBuffer(args.DepthBuffer.Value);
        }

        pass->ClearFunction = args.ClearFunction != null ? args.ClearFunction : &ClearFunctionStub;

        return index + HandleOffset;
    }

    private static RootSignatureBuilder CreateDefaultRootSignatureBuilder(byte numberOfInputs) =>
        new RootSignatureBuilder()
            // Predefined slots for bindless resources.
            .WithDecriptorRange(6, register: 0, space: 10)
            .WithConstant(numberOfInputs, ShaderVisibility.Pixel, register: 0, space: 10)
            .WithConstantBuffer(ConstantBufferFlags.Static, ShaderVisibility.All, register: 0, space: 11)
            .WithSampler(SamplerState.Point, ShaderVisibility.Pixel, register: 0, space: 10)
            .WithSampler(SamplerState.Linear, ShaderVisibility.Pixel, register: 1, space: 10)
            .WithSampler(SamplerState.Shadow, ShaderVisibility.Pixel, register: 2, space: 10);

    public readonly bool Begin(in Handle<RenderPass> handle, out CommandList commandList)
    {
        Unsafe.SkipInit(out commandList);
        if (!_isReady)
        {
            return false;
        }
        var index = handle.Value - HandleOffset;
        var pass = _renderPasses.AsPointer() + index;

        //NOTE(Jens): We always reset the command list with the first Pipeline state (if multiple are present)
        pass->CommandList = commandList = _commandQueue->GetCommandList(pass->PipelineStates[0]);
        pass->CommandList.SetGraphicsRootSignature(_resourceManager->Access(pass->RootSignature)->Resource);
        pass->CommandList.SetTopology(pass->Topology switch
        {
            PrimitiveTopology.Line => D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_LINELIST,
            PrimitiveTopology.TriangleStrip => D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP,
            _ => D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST
        });
        pass->CommandList.SetDescriptorHeap(_d3d12Allocator->SRV.Heap);
        pass->CommandList.SetGraphicsRootDescriptorTable((uint)RootSignatureIndex.Texture2D, _d3d12Allocator->SRV.GPUStart);
        pass->CommandList.SetViewport(&pass->Viewport);
        pass->CommandList.SetScissorRect(&pass->ScissorRect);

        //NOTE(Jens): A lot of stack allocs, do we need all of them? :O
        TitanList<D3D12_RESOURCE_BARRIER> barriers = stackalloc D3D12_RESOURCE_BARRIER[10];
        TitanList<D3D12_CPU_DESCRIPTOR_HANDLE> renderTargets = stackalloc D3D12_CPU_DESCRIPTOR_HANDLE[(int)pass->Outputs.Length];
        TitanList<Ptr<Texture>> renderTargetTextures = stackalloc Ptr<Texture>[(int)pass->Outputs.Length];
        TitanList<int> inputTextures = stackalloc int[(int)pass->Inputs.Length + 1]; // we add 1 for depth buffer.

        foreach (ref readonly var output in pass->Outputs.AsReadOnlySpan())
        {
            var isBackbuffer = output == _backbufferHandle;

            //TODO(Jens): Don't transition resource to the Common state.
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

        var frameDataBuffer = _resourceManager->Access(_frameDataConstantBuffer);
        commandList.SetGraphicsRootConstantBuffer((uint)RootSignatureIndex.FrameDataIndex, frameDataBuffer);

        Texture* depthBuffer = null;
        if (pass->DepthBuffer.IsValid)
        {
            depthBuffer = _resourceManager->Access(pass->DepthBuffer);
            commandList.SetRenderTargets(renderTargets, renderTargets.Count, pass->DepthBufferMode == DepthBufferMode.Write ? &depthBuffer->DSV.CPU : null);
            if (pass->DepthBufferMode == DepthBufferMode.Read)
            {
                Debug.Assert(depthBuffer->SRV.IsValid);
                inputTextures.Add(depthBuffer->SRV.Index);
                barriers.Add(D3D12Helpers.Transition(depthBuffer->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_WRITE, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_READ));
            }
        }
        else
        {
            commandList.SetRenderTargets(renderTargets, renderTargets.Count);
        }

        if (!inputTextures.IsEmpty)
        {
            commandList.SetGraphicsRootConstants((uint)RootSignatureIndex.InputTexturesIndex, inputTextures);
        }
        if (!barriers.IsEmpty)
        {
            commandList.ResourceBarriers(barriers);
        }
        pass->ClearFunction(renderTargetTextures, depthBuffer, commandList);

        return true;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly CommandList GetCommandList(in Handle<RenderPass> handle)
    {
        var index = handle.Value - HandleOffset;
        return _renderPasses[index].CommandList;
    }

    public readonly Handle<RootSignature> GetRootSignature(in Handle<RenderPass> handle)
    {
        var index = handle.Value - HandleOffset;
        return _renderPasses[index].RootSignature;
    }

    public readonly Handle<Texture> GetDepthBuffer(in Handle<RenderPass> handle)
    {
        var index = handle.Value - HandleOffset;
        return _renderPasses[index].DepthBuffer;
    }

    public readonly void SetPipelineState(in Handle<RenderPass> handle, int pipelineStateIndex)
    {
        var index = handle.Value - HandleOffset;
        ref readonly var pass = ref _renderPasses[index];
        Debug.Assert(pipelineStateIndex >= 0 && pipelineStateIndex < pass.PipelineStates.Length);
        var pipelineState = _resourceManager->Access(pass.PipelineStates[pipelineStateIndex]);
        pass.CommandList.SetPipelineState(pipelineState);
    }

    public readonly void End(in Handle<RenderPass> handle)
    {
        if (!IsReady || handle.IsInvalid)
        {
            return;
        }

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
            //NOTE(Jens): Transition back to common is bad becasue its using a lot of resources (or slow). We need a better way to handle this.
            var texture = _resourceManager->Access(input);
            barriers.Add(D3D12Helpers.Transition(texture->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_ALL_SHADER_RESOURCE, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_COMMON));
        }

        if (pass->DepthBuffer.IsValid && pass->DepthBufferMode == DepthBufferMode.Read)
        {
            var depthBuffer = _resourceManager->Access(pass->DepthBuffer);
            barriers.Add(D3D12Helpers.Transition(depthBuffer->Resource, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_READ, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_DEPTH_WRITE));
        }

        if (!barriers.IsEmpty)
        {
            pass->CommandList.ResourceBarriers(barriers);
        }
        pass->CommandList.Close();
    }

    [System(SystemStage.First)]
    internal static void OnNewFrame(ref RenderGraph graph, in D3D12ResourceManager resourceManager, in DXGISwapchain swapchain, in InputState inputState)
    {
        if (graph._isReady)
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

        graph.SetDefaultViewPorts();
        graph.SortRenderGraph();

        graph._isReady = true;
        graph._backbufferHandle = swapchain.CurrentBackbuffer;
    }

    private void SetDefaultViewPorts()
    {
        foreach (ref var pass in _renderPasses.AsSpan().Slice(0, _renderPassCount))
        {
            if (!pass.UseDefaultViewport)
            {
                continue;
            }

            pass.Viewport = new()
            {
                Width = EngineState.WindowWidth,
                Height = EngineState.WindowHeight,
                MaxDepth = 1.0f,
                MinDepth = 0.0f,
                TopLeftX = 0,
                TopLeftY = 0,
            };
            pass.ScissorRect = new()
            {
                Bottom = EngineState.WindowHeight,
                Right = EngineState.WindowWidth,
                Top = 0,
                Left = 0
            };
        }
    }


    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    internal static void ExeucuteCommandLists(in RenderGraph graph, in D3D12CommandQueue commandQueue, in CameraSystem cameraSystem, in Window window)
    {
        if (!graph._isReady)
        {
            return;
        }

        ref readonly var camera = ref cameraSystem.GetCurrentCamera();

        //NOTE(Jens): We can probably do this in some nicer way :) but works for now.
        graph._frameDataGPU.WriteSingle(new FrameData
        {
            WorldMatrix = camera.WorldMatrix,
            ViewProjection = camera.ViewProjectionMatrixTransposed,
            CameraPosition = camera.Position,
            WindowHeight = (uint)window.Height,
            WindowWidth = (uint)window.Width,
            Time = GameTime.TotalTimeSeconds
        });

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
            for (var shaderIndex = 0; shaderIndex < pass.PixelShaders.Length; ++shaderIndex)
            {
                if (pass.VertexShaders[shaderIndex].IsValid && !_assetsManager.IsLoaded(pass.VertexShaders[shaderIndex]))
                {
                    return false;
                }

                if (pass.PixelShaders[shaderIndex].IsValid && !_assetsManager.IsLoaded(pass.PixelShaders[shaderIndex]))
                {
                    return false;
                }
            }
        }

        return true;
    }


    [System(SystemStage.Last, SystemExecutionType.Inline)]
    internal static void Last(ref RenderGraph graph, EventReader<WindowResizeEvent> resizeEvent)
    {
        if (!resizeEvent.HasEvents)
        {
            return;
        }
        foreach (var _ in resizeEvent)
        {
            Logger.Warning<RenderGraph>("This is a hack, rethink this solution. Function = Last");
            graph.SetDefaultViewPorts();
            break;
        }
    }


    private bool CreatePipelineStates()
    {
        foreach (ref var pass in _renderPasses.AsSpan()[.._renderPassCount])
        {
            for (var shaderIndex = 0; shaderIndex < pass.PipelineStates.Length; ++shaderIndex)
            {
                pass.PipelineStates[shaderIndex] = CreatePipelineState(pass, shaderIndex);
                if (pass.PipelineStates[shaderIndex].IsInvalid)
                {
                    Logger.Error<RenderGraph>($"Failed to create the pipeline state for render pass. Name = {pass.Name.GetString()}");
                    return false;
                }
#if HOT_RELOAD_ASSETS
                pass.ShaderHashes[shaderIndex] = HashFromShaders(pass.VertexShaders[shaderIndex], pass.PixelShaders[shaderIndex]);
#endif
            }

        }

        return true;
    }

    private Handle<PipelineState> CreatePipelineState(in RenderPass renderPass, int shaderIndex)
    {
        return _resourceManager->CreatePipelineState(new CreatePipelineStateArgs
        {
            BlendState = renderPass.BlendState,
            Depth = GetDeptStencilArgs(renderPass, _resourceManager),
            RenderTargets = renderPass.Outputs,
            RootSignature = renderPass.RootSignature,
            VertexShader = _assetsManager.Get(renderPass.VertexShaders[shaderIndex]).ShaderByteCode,
            PixelShader = renderPass.PixelShaders[shaderIndex].IsValid ? _assetsManager.Get(renderPass.PixelShaders[shaderIndex]).ShaderByteCode : TitanBuffer.Empty,
            Topology = renderPass.Topology switch
            {
                PrimitiveTopology.Line => D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_LINE,
                _ => D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE
            },
            CullMode = renderPass.CullMode,
            FillMode = renderPass.FillMode
        });

        static DepthStencilArgs GetDeptStencilArgs(in RenderPass pass, D3D12ResourceManager* resourceManager)
        {
            if (pass.DepthBuffer.IsInvalid || pass.DepthBufferMode == DepthBufferMode.Read)
            {
                return default;
            }

            return new()
            {
                Format = resourceManager->Access(pass.DepthBuffer)->Format,
                DepthEnabled = true,
                StencilEnabled = false, //TODO(Jens): Add support for stencil tests
            };
        }
    }

    private void SortRenderGraph()
    {
        Span<GraphDependencies> graph = stackalloc GraphDependencies[_renderPassCount];

        // Go through all passes and map the dependencies by looking at the output from one pass and comparing to it the input of another pass.
        // If it's a match, add the dependant pass to the graph, and increaes the inDegree of the dependant pass
        for (var outer = 0; outer < _renderPassCount; ++outer)
        {
            var depthBuffer = _renderPasses[outer].DepthBuffer;
            // if we have a depthbuffer, check the depthbuffer with mode Read for the rest of the passes.
            if (depthBuffer.IsValid && _renderPasses[outer].DepthBufferMode == DepthBufferMode.Write)
            {
                for (var inner = 0; inner < _renderPassCount; ++inner)
                {
                    if (inner == outer)
                    {
                        continue;
                    }

                    if (_renderPasses[inner].DepthBufferMode == DepthBufferMode.Read && _renderPasses[inner].DepthBuffer == depthBuffer)
                    {
                        graph[outer].AddDependency(inner);
                        graph[inner].InDegree++;
                    }
                }
            }

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

                    if (Contains(_renderPasses[inner].Outputs, output))
                    {
                        Debug.Assert(_renderPasses[inner].Order != _renderPasses[outer].Order, "Multiple render passes with the same output must have an Order set.");
                        if (_renderPasses[inner].Order > _renderPasses[outer].Order)
                        {
                            graph[outer].AddDependency(inner);
                            graph[inner].InDegree++;
                        }
                    }

                    if (Contains(_renderPasses[inner].Inputs, depthBuffer))
                    {
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

#if DEBUG
        for (var i = 0; i < _groupCount; ++i)
        {
            Logger.Trace<RenderGraph>($"Render Group {i}");
            ref readonly var group = ref groups[i];
            for (var j = 0; j < group.Count; ++j)
            {
                Logger.Trace<RenderGraph>($"Pass: {_sortedPasses[j + group.Offset].Get()->Name.GetString()}");
            }
        }
#endif
        static bool Contains(ReadOnlySpan<Handle<Texture>> textures, in Handle<Texture> handle)
        {
            if (handle.IsInvalid)
            {
                return false;
            }
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

    public readonly void DestroyPass(Handle<RenderPass> handle)
    {
        Debug.Assert(handle.IsValid);
        var pass = _renderPasses.GetPointer(handle - HandleOffset);
        for (var i = 0; i < pass->PixelShaders.Length; ++i)
        {
            if (pass->VertexShaders[i].IsValid)
            {
                _assetsManager.Unload(ref pass->VertexShaders[i]);
            }

            if (pass->PixelShaders[i].IsValid)
            {
                _assetsManager.Unload(ref pass->PixelShaders[i]);
            }

            if (pass->PipelineStates[i].IsValid)
            {
                _resourceManager->DestroyPipelineState(pass->PipelineStates[i]);
            }
        }

        _resourceManager->DestroyRootSignature(pass->RootSignature);
        *pass = default;
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(ref RenderGraph graph, IMemoryManager memoryManager, in D3D12ResourceManager resourceManager)
    {
        memoryManager.FreeAllocator(graph._allocator);
        //TODO(Jens): Add delayed resource disposal
        resourceManager.Unmap(graph._frameDataGPU);
        //resourceManager.DestroyBuffer(graph._frameDataConstantBuffer);

        graph = default;
    }



#if HOT_RELOAD_ASSETS
    [System(SystemStage.Last)]
    internal static void CheckForDirtyShaders(ref RenderGraph graph)
    {
        if (graph._isReady == false)
        {
            return;
        }

        foreach (ref var pass in graph._renderPasses.AsSpan()[..graph._renderPassCount])
        {
            for (var shaderIndex = 0; shaderIndex < pass.ShaderHashes.Length; ++shaderIndex)
            {
                var hash = graph.HashFromShaders(pass.VertexShaders[shaderIndex], pass.PixelShaders[shaderIndex]);
                if (hash != pass.ShaderHashes[shaderIndex])
                {
                    Logger.Warning($"Hash is different, recreating pipeline state. Pass = {pass.Name.GetString()} Shader Index = {shaderIndex}");
                    //graph._isReady = false;

                    pass.PipelineStates[shaderIndex] = graph.CreatePipelineState(pass, shaderIndex);
                    pass.ShaderHashes[shaderIndex] = hash;
                }
            }

        }
    }

    private ulong HashFromShaders(AssetHandle<ShaderAsset> shader1, AssetHandle<ShaderAsset> shader2)
    {
        var a = _assetsManager.Get(shader1).ShaderByteCode;
        var b = shader2.IsValid ? _assetsManager.Get(shader2).ShaderByteCode : a;
        var c = (ulong)a.Size + (ulong)a.AsPointer();
        var d = (ulong)b.Size + (ulong)b.AsPointer();
        return unchecked(c + d);
    }
#endif
}
