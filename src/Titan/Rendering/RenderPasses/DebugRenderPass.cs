using System.Numerics;
using System.Runtime.CompilerServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Rendering.RenderPasses;

[UnmanagedResource]
internal unsafe partial struct DebugRenderPass
{
    private Handle<RenderPass> Handle;
    private Handle<GPUBuffer> LineBuffer;
    private MappedGPUResource<Line> LineBufferGPU;
    private Inline2048<Line> Lines;
    private int Count;

    private const uint PassDataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    [System(SystemStage.Init)]
    public static void Init(DebugRenderPass* pass, in D3D12ResourceManager resourceManager, in RenderGraph graph)
    {
        var args = new CreateRenderPassArgs
        {
            FillMode = FillMode.Solid,
            BlendState = BlendStateType.Disabled,
            ClearFunction = &ClearFunction,
            CullMode = CullMode.None,
            DepthBuffer = null,
            Inputs = [],
            Outputs = [BuiltInRenderTargets.Debug],
            Topology = PrimitiveTopology.Line,
            RootSignatureBuilder = builder => builder
                .WithDecriptorRange(1, register: 0, space: 0),
            PixelShader = EngineAssetsRegistry.Shaders.ShaderDebugPixel,
            VertexShader = EngineAssetsRegistry.Shaders.ShaderDebugVertex
        };

        pass->Handle = graph.CreatePass("DEBUG_LINE", args);

        pass->LineBuffer = resourceManager.CreateBuffer(CreateBufferArgs.Create<Line>((uint)pass->Lines.Size, BufferType.Structured, cpuVisible: true, shaderVisible: true));
        if (!resourceManager.TryMapBuffer(pass->LineBuffer, out pass->LineBufferGPU))
        {
            Logger.Error<DebugRenderPass>("Failed to map the debug line buffer.");
            return;
        }

        pass->Lines = default;
        pass->Count = 0;

        DebugDraw.DebugAPI = pass;
    }


    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
        => commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.Debug.OptimizedClearColor);

    public readonly void DrawLine(in Vector3 from, in Vector3 to, in ColorRGB colorRGBFrom, in ColorRGB colorRGBTo)
    {
        ref var count = ref Unsafe.AsRef(in Count);
        var index = Interlocked.Increment(ref count) - 1;
        *Lines.GetPointer(index) = new(from, to, colorRGBFrom, colorRGBTo);
    }

    [System(SystemStage.PreUpdate)]
    public static void Begin(ref DebugRenderPass pass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.Begin(pass.Handle, out var commandList))
        {
            return;
        }
        commandList.SetGraphicsRootDescriptorTable(PassDataIndex, resourceManager.Access(pass.LineBuffer));
        pass.Count = 0;
    }

    [System(SystemStage.PostUpdate)]
    public static void Render(in DebugRenderPass pass, in RenderGraph graph)
    {
        if (!graph.IsReady)
        {
            return;
        }

        if (pass.Count > 0)
        {
            pass.LineBufferGPU.Write(pass.Lines.AsReadOnlySpan()[..pass.Count]);
            var commandList = graph.GetCommandList(pass.Handle);

            commandList.DrawInstanced(2, (uint)pass.Count);
        }

        graph.End(pass.Handle);
    }

    private record struct Line(Vector3 Start, Vector3 Stop, ColorRGB color, ColorRGB color2);

    public readonly void DrawAABB(in AABB boundingBox, in Color color)
    {
        var min = boundingBox.Min;
        var max = boundingBox.Max;

        DrawLine(min, new(min.X, min.Y, max.Z), color, color);
        DrawLine(min, new(min.X, max.Y, min.Z), color, color);
        DrawLine(min, new(max.X, min.Y, min.Z), color, color);

        DrawLine(max, new(max.X, max.Y, min.Z), color, color);
        DrawLine(max, new(max.X, min.Y, max.Z), color, color);
        DrawLine(max, new(min.X, max.Y, max.Z), color, color);

        DrawLine(new(min.X, min.Y, max.Z), new(min.X, max.Y, max.Z), color, color);
        DrawLine(new(max.X, max.Y, min.Z), new(max.X, min.Y, min.Z), color, color);
        DrawLine(new(min.X, max.Y, min.Z), new(max.X, max.Y, min.Z), color, color);
        DrawLine(new(min.X, max.Y, max.Z), new(min.X, max.Y, min.Z), color, color);
        DrawLine(new(min.X, min.Y, max.Z), new(max.X, min.Y, max.Z), color, color);
        DrawLine(new(max.X, min.Y, min.Z), new(max.X, min.Y, max.Z), color, color);
    }
}
