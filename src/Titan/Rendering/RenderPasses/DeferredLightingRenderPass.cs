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
using Titan.Application;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Input;
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
    private static Vector3[] HardcodedLights =
    [

        new(1f), // 100% brightness
        new(0.4f, 0.4f, 0.4f), // Neutral gray, 40% brightness
        new(0.5f, 0.45f, 0.35f), // Slightly warm tone
        new(0.3f, 0.35f, 0.4f), // Cool tone with bluish tint
        new(0.1f, 0.1f, 0.2f), // Dim with a bluish hue
        new(0.05f, 0.05f, 0.1f) // Minimal light with a hint of blue
    ];

    private static int LightIndex = 5;

    private Handle<RenderPass> PassHandle;
    private const uint RootConstantPassData = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;
    private const uint RootConstantLightInstanceIndex = RootConstantPassData + 1;

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
                .WithConstant(3, ShaderVisibility.Pixel, register: 0, space: 0)
                .WithDecriptorRange(1, register: 0, space: 0),
            BlendState = BlendStateType.Additive,
            CullMode = CullMode.Back,
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

        var lightsBuffer = resourceManager.Access(pass->LightInstanceHandles[EngineState.FrameIndex]);
        commandList.SetGraphicsRootDescriptorTable(RootConstantLightInstanceIndex, lightsBuffer);
        //TODO(Jens): This should be set somewhere else

        commandList.SetGraphicsRootConstant(RootConstantPassData, HardcodedLights[LightIndex]);
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
    public static void EndPass(in DeferredLightingRenderPass pass, in RenderGraph graph, in D3D12ResourceManager resourceManager, in InputState inputState)
    {
        if (!graph.IsReady)
        {
            return;
        }

        if (inputState.IsKeyReleased(KeyCode.L))
        {
            LightIndex = (LightIndex + 1) % HardcodedLights.Length;
        }

        var commandList = graph.GetCommandList(pass.PassHandle);

        if (pass.LightInstances > 0)
        {
            var lights = pass
                .CPULights
                .Slice(0, pass.LightInstances)
                .AsReadOnlySpan();

            pass
                .GPULights[EngineState.FrameIndex]
                .Write(lights);

            commandList.DrawInstanced(3, pass.LightInstances);
        }
        else
        {
            pass
                .GPULights[EngineState.FrameIndex]
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
