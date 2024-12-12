using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Resources;
using Titan.Systems;
using Titan.Core.Logging;
using System.Numerics;
using Titan.Core.Maths;
using Titan.Core.Memory;
using static Titan.Assets.EngineAssetsRegistry.Shaders;

namespace Titan.Rendering.RenderPasses;

[StructLayout(LayoutKind.Sequential)]
internal struct LightInstanceData
{
    public Vector3 Position;
    public Vector3 Direction;
    public ColorRGB Color;
    public float IntensityOrRadius;
    public unsafe fixed float Padding[2];
}

[UnmanagedResource]
internal unsafe partial struct DeferredLightingRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint RootConstantLightInstanceIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    private Inline2<Handle<GPUBuffer>> LightInstanceHandles;
    private Inline2<MappedGPUResource<LightInstanceData>> GPULights;

    private TitanArray<LightInstanceData> CPULights;
    private uint LightInstances;

    private static readonly LightInstanceData DefaultLight = default;

    [System(SystemStage.Init)]
    public static void Init(DeferredLightingRenderPass* renderPass, in RenderGraph graph, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();
        var maxLights = config.Resources.MaxLights;

        renderPass->PassHandle = graph.CreatePass("DeferredLighting", new()
        {
            RootSignatureBuilder = static builder => builder
                .WithDecriptorRange(1, register: 0, space: 0),
            BlendState = BlendStateType.Additive,
            Outputs = [BuiltInRenderTargets.DeferredLighting],
            Inputs =
            [
                BuiltInRenderTargets.GBufferPosition,
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular
            ],

            PixelShader = ShaderDeferredLightingPixel,
            VertexShader = ShaderDeferredLightingVertex,
            ClearFunction = &ClearFunction
        });

        Logger.Trace<DeferredLightingRenderPass>($"Max Lights = {maxLights}");
        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            renderPass->LightInstanceHandles[i] = resourceManager.CreateBuffer(CreateBufferArgs.Create<LightInstanceData>(maxLights, BufferType.Structured, cpuVisible: true, shaderVisible: true));
            if (renderPass->LightInstanceHandles[i].IsInvalid)
            {
                Logger.Error<DeferredLightingRenderPass>("Failed to create the buffer for Lights.");
                return;
            }

            if (!resourceManager.TryMapBuffer(renderPass->LightInstanceHandles[i], out renderPass->GPULights[i]))
            {
                Logger.Error<DeferredLightingRenderPass>("Failed to map the Instance Buffer");
                return;
            }
        }

        if (!memoryManager.TryAllocArray(out renderPass->CPULights, maxLights))
        {
            Logger.Error<DeferredLightingRenderPass>($"Failed to allocate memory for lights. MaxLights = {maxLights}. Size = {sizeof(LightInstanceData) * maxLights} bytes");
            return;
        }

        renderPass->LightInstances = 0;
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.DeferredLighting.OptimizedClearColor);
    }

    [System(SystemStage.PreUpdate)]
    public static void BeginPass(DeferredLightingRenderPass* pass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.Begin(pass->PassHandle, out var commandList))
        {
            return;
        }

        var lightsBuffer = resourceManager.Access(pass->LightInstanceHandles[graph.FrameIndex]);
        commandList.SetGraphicsRootDescriptorTable(RootConstantLightInstanceIndex, lightsBuffer);

        pass->LightInstances = 0;
    }

    [System]
    public static void RenderLights(DeferredLightingRenderPass* pass, in RenderGraph graph, ReadOnlySpan<Light> lights, ReadOnlySpan<Transform3D> transforms)
    {
        if (!graph.IsReady)
        {
            return;
        }

        ref var index = ref pass->LightInstances;
        var stagingBuffer = pass->CPULights;
        var count = lights.Length;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var light = ref lights[i];
            if (!light.Active)
            {
                continue;
            }

            ref readonly var transform = ref transforms[i];
            stagingBuffer[index++] = new()
            {
                Color = light.Color,
                Direction = light.Direction,
                IntensityOrRadius = light.Radius,
                Position = transform.Position
            };
        }
    }

    [System]
    public static void EndPass(in DeferredLightingRenderPass pass, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.IsReady)
        {
            return;
        }

        var commandList = graph.GetCommandList(pass.PassHandle);

        if (pass.LightInstances > 0)
        {
            var frameIndex = graph.FrameIndex;
            var lights = pass
                .CPULights
                .Slice(0, pass.LightInstances)
                .AsReadOnlySpan();

            pass
                .GPULights[frameIndex]
                .Write(lights);

            commandList.DrawInstanced(3, pass.LightInstances);
        }
        else
        {
            pass
                .GPULights[graph.FrameIndex]
                .WriteSingle(DefaultLight);
            commandList.DrawInstanced(3, 1);
        }

        graph.End(pass.PassHandle);
    }


    [System(SystemStage.Shutdown)]
    public static void Shutdown(DeferredLightingRenderPass* pass, in RenderGraph graph, in DXGISwapchain _) //NOTE(Jens): Get a Swapchain reference to make sure everything has been flushed before releasing it. A hack.. Need a better system for doing this.
    {
        Logger.Warning<DeferredLightingRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }
}
