using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Materials;
using Titan.Meshes;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;
using static Titan.Assets.EngineAssetsRegistry.Shaders;

namespace Titan.Rendering.RenderPasses;

/// <summary>
/// This is stored on the GPU, have to be 16 byte aligned.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MeshInstanceData
{
    public Matrix4x4 ModelMatrix;
    public int MaterialIndex;
    private unsafe fixed float _padding[3];
}

[UnmanagedResource]
internal unsafe partial struct GBufferRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint PassDataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;
    private const uint VertexBufferIndex = PassDataIndex + 1;
    private const uint IndexBufferIndex = VertexBufferIndex + 1;
    private const uint MeshInstanceIndex = IndexBufferIndex + 1;
    private const uint MaterialsInstanceIndex = MeshInstanceIndex + 1;

    private uint MeshInstances;
    private TitanArray<MeshInstanceData> StagingBuffer;
    private Inline2<Handle<GPUBuffer>> MeshInstancesHandles;
    private Inline2<MappedGPUResource<MeshInstanceData>> GPUMeshIntances;

    [System(SystemStage.Init)]
    public static void Init(GBufferRenderPass* renderPass, in RenderGraph renderGraph, in AssetsManager assetsManager, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager)
    {
        const uint MaxStagedMeshes = 20 * 1024;

        var passArgs = new CreateRenderPassArgs
        {
            RootSignatureBuilder = static builder => builder
                .WithRootConstant<uint>() // the index of the Meshinstance we're rendering.
                .WithDecriptorRange(1, space: 0) // Vertex buffer
                .WithDecriptorRange(1, space: 1) // IndexBuffer
                .WithDecriptorRange(1, space: 2) // MeshInstance
                .WithDecriptorRange(1, space: 3) // MaterialsInstance
            ,
            BlendState = BlendStateType.AlphaBlend, //NOTE(Jens): maybe it should be disabled?
            CullMode = CullMode.Back,
            FillMode = FillMode.Solid,
            Outputs =
            [
                BuiltInRenderTargets.GBufferPosition,
                BuiltInRenderTargets.GBufferAlbedo,
                BuiltInRenderTargets.GBufferNormal,
                BuiltInRenderTargets.GBufferSpecular,
            ],
            Inputs = [],
            DepthBuffer = BuiltInDepthsBuffers.GbufferDepthBuffer,
            ClearFunction = &ClearFunction,
            VertexShader = ShaderGBufferVertex,
            PixelShader = ShaderGBufferPixel
        };

        renderPass->PassHandle = renderGraph.CreatePass("GBuffer", passArgs);

        if (!memoryManager.TryAllocArray(out renderPass->StagingBuffer, MaxStagedMeshes))
        {
            Logger.Error<GBufferRenderPass>("Failed to allocate memory for the staging buffer.");
            return;
        }

        for (var i = 0; i < GlobalConfiguration.MaxRenderFrames; ++i)
        {
            renderPass->MeshInstancesHandles[i] = resourceManager.CreateBuffer(CreateBufferArgs.Create<MeshInstanceData>(MaxStagedMeshes, BufferType.Structured, cpuVisible: true, shaderVisible: true));
            if (renderPass->MeshInstancesHandles[i].IsInvalid)
            {
                Logger.Error<GBufferRenderPass>("Failed to allocate the Instance Buffer");
                return;
            }
            if (!resourceManager.TryMapBuffer(renderPass->MeshInstancesHandles[i], out renderPass->GPUMeshIntances[i]))
            {
                Logger.Error<GBufferRenderPass>("Failed to map the Instance Buffer");
                return;
            }
        }

        renderPass->MeshInstances = 0;
    }

    private static void ClearFunction(ReadOnlySpan<Ptr<Texture>> renderTargets, TitanOptional<Texture> depthBuffer, in CommandList commandList)
    {
        commandList.ClearRenderTargetView(renderTargets[0], BuiltInRenderTargets.GBufferPosition.OptimizedClearColor);
        commandList.ClearRenderTargetView(renderTargets[1], BuiltInRenderTargets.GBufferAlbedo.OptimizedClearColor);
        commandList.ClearRenderTargetView(renderTargets[2], BuiltInRenderTargets.GBufferNormal.OptimizedClearColor);
        commandList.ClearRenderTargetView(renderTargets[3], BuiltInRenderTargets.GBufferSpecular.OptimizedClearColor);

        if (depthBuffer.HasValue)
        {
            commandList.ClearDepthStencilView(depthBuffer.AsPtr(), D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH, 1, 0, 0, null);
        }
    }


    [System(SystemStage.PreUpdate)]
    public static void BeginRenderPass(GBufferRenderPass* pass, in RenderGraph graph, in Window window, in MeshSystem meshSystem, in MaterialsSystem materialsSystem, in D3D12ResourceManager resourceManager)
    {
        if (!graph.Begin(pass->PassHandle, out var commandList))
        {
            return;
        }

        pass->MeshInstances = 0;

        var meshBuffer = resourceManager.Access(pass->MeshInstancesHandles[graph.FrameIndex]);
        var materialBuffer = resourceManager.Access(materialsSystem.GetMaterialsGPUHandle(graph.FrameIndex));
        var indexBuffer = resourceManager.Access(meshSystem.GetIndexBufferHandle());
        var vertexBuffer = resourceManager.Access(meshSystem.GetVertexBufferHandle());

        commandList.SetIndexBuffer(indexBuffer); // remove when we support indexing into the index buffer inside the shader.
        commandList.SetGraphicsRootDescriptorTable(VertexBufferIndex, vertexBuffer);
        commandList.SetGraphicsRootDescriptorTable(MeshInstanceIndex, meshBuffer);
        commandList.SetGraphicsRootDescriptorTable(MaterialsInstanceIndex, materialBuffer);
    }

    /// <summary>
    /// Renders the meshes. This function will be called multiple times depending on the archetypes
    /// </summary>
    [System]
    public static void RenderMeshes(GBufferRenderPass* pass, ReadOnlySpan<Mesh> meshes, in MeshSystem meshSystem, ReadOnlySpan<Transform3D> transforms, in AssetsManager assetsManager, in RenderGraph graph, in D3D12ResourceManager resourceManager)
    {
        if (!graph.IsReady)
        {
            return;
        }

        var commandList = graph.GetCommandList(pass->PassHandle);

        var count = meshes.Length;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var mesh = ref meshes[i];
            ref readonly var transform = ref transforms[i];

            //TODO(Jens): Add frustrum culling

            var meshData = meshSystem.Access(mesh.MeshIndex);
            var meshInstanceIndex = pass->MeshInstances++;
            ref var meshInstanceData = ref pass->StagingBuffer[meshInstanceIndex];
            meshInstanceData.MaterialIndex = mesh.MaterialIndex;

            //NOTE(Jens): Consider implementing a TitanMatrix4x4 instead of using the built in. A lot of work, but calling Transponse on every Matrix might be bad  as well :|
            //NOTE(Jens): another option is to use row_major in HLSL, this is probably not very optimized either.
            meshInstanceData.ModelMatrix = Matrix4x4.Transpose(Matrix4x4.CreateScale(transform.Scale) *
                                           Matrix4x4.CreateFromQuaternion(transform.Rotation) *
                                           Matrix4x4.CreateTranslation(transform.Position)
                                           )
                                           ;

            //TODO: Implement GPU instancing

            commandList.SetGraphicsRootConstant(PassDataIndex, meshInstanceIndex);
            for (var index = 0; index < meshData->SubMeshCount; ++index)
            {
                ref readonly var submesh = ref meshData->SubMeshes[index];
                commandList.DrawIndexedInstanced(submesh.IndexCount, 1, submesh.IndexStartLocation, 0);
            }

        }
    }

    /// <summary>
    /// Ends the pass and closes the command list.
    /// </summary>
    [System]
    public static void EndPass(in GBufferRenderPass pass, in RenderGraph graph)
    {
        pass.GPUMeshIntances[graph.FrameIndex].Write(pass.StagingBuffer.Slice(0, pass.MeshInstances));

        graph.End(pass.PassHandle);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(GBufferRenderPass* pass, in RenderGraph graph, in DXGISwapchain _) //NOTE(Jens): Get a Swapchain reference to make sure everything has been flushed before releasing it. A hack.. Need a better system for doing this.
    {
        Logger.Warning<GBufferRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }
}

