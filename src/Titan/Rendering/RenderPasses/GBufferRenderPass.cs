using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.ECS.Components;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.Storage;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.RenderPasses;


[UnmanagedResource]
internal unsafe partial struct GBufferRenderPass
{
    private Handle<RenderPass> PassHandle;
    private const uint PassDataIndex = (uint)RenderGraph.RootSignatureIndex.CustomIndexStart;

    [System(SystemStage.Init)]
    public static void Init(GBufferRenderPass* renderPass, in RenderGraph renderGraph, in AssetsManager assetsManager, IMemoryManager memoryManager)
    {
        var passArgs = new CreateRenderPassArgs
        {
            RootSignatureBuilder = static builder => builder
                .WithRootConstant<uint>() // the index of the Meshinstance we're rendering.
                .WithDecriptorRange(1, space: 0) // Vertex buffer
                .WithDecriptorRange(1, space: 1) // IndexBuffer
                .WithDecriptorRange(1, space: 2) // MeshInstance
            ,
            BlendState = BlendStateType.AlphaBlend, //NOTE(Jens): maybe it should be disabled?
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
            VertexShader = EngineAssetsRegistry.ShaderGBufferVertex,
            PixelShader = EngineAssetsRegistry.ShaderGBufferPixel
        };

        renderPass->PassHandle = renderGraph.CreatePass("GBuffer", passArgs);
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
    public static void BeginRenderPass(GBufferRenderPass* pass, in RenderGraph graph, in Window window, in MeshStorage meshStorage, in D3D12ResourceManager resourceManager)
    {
        if (!graph.Begin(pass->PassHandle, out var commandList))
        {
            return;
        }

        commandList.SetIndexBuffer(resourceManager.Access(meshStorage.IndexBufferHandle)); // remove when we support indexing into the index buffer inside the shader.

        var vertexBufferIndex = resourceManager.Access(meshStorage.VertexBufferHandle)->SRV.GPU;
        var meshInstanceIndex = resourceManager.Access(meshStorage.MeshInstancesHandle)->SRV.GPU;
        //var indexBufferIndex = resourceManager.Access(meshStorage.IndexBufferHandle)->SRV.GPU;
        commandList.SetGraphicsRootDescriptorTable(PassDataIndex + 1, vertexBufferIndex);
        //commandList.SetGraphicsRootDescriptorTable(PassDataIndex + 2, indexBufferIndex);
        commandList.SetGraphicsRootDescriptorTable(PassDataIndex + 3, meshInstanceIndex);

        //NOTE(Jens): Not sure what to do with these. 
        D3D12_VIEWPORT viewPort = new()
        {
            Height = window.Height,
            Width = window.Width,
            MaxDepth = 1.0f,
            MinDepth = 0,
            TopLeftX = 0,
            TopLeftY = 0
        };

        D3D12_RECT rect = new()
        {
            Bottom = window.Height,
            Right = window.Width,
            Left = 0,
            Top = 0
        };
        commandList.SetScissorRect(&rect);
        commandList.SetViewport(&viewPort);
    }

    /// <summary>
    /// Renders the meshes. This function will be called multiple times depending on the archetypes
    /// </summary>
    [System]
    public static void RenderMeshes(GBufferRenderPass* pass, ReadOnlySpan<Mesh> meshes, ReadOnlySpan<Transform3D> transform, in AssetsManager assetsManager, in MeshStorage storage, in RenderGraph graph, in D3D12ResourceManager resourceManager)
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
            Debug.Assert(mesh.InstanceIndex.IsValid, "Unexpected order of initializing the instance handle.. Fix!");
            if (mesh.MeshData == null)
            {
                continue;
            }

            if (!assetsManager.IsLoaded(mesh.TextureAsset))
            {
                continue;
            }

            var textureIndex = resourceManager.Access(assetsManager.Get(mesh.TextureAsset).Handle)->SRV.Index;
            storage.UpdateMeshInstance(mesh.InstanceIndex, new MeshInstance
            {
                AlbedoIndex = textureIndex, // todo: we need a material system for this to work.
                ModelMatrix = Matrix4x4.Identity
            });

            commandList.SetGraphicsRootConstant(PassDataIndex, mesh.InstanceIndex.Value);
            foreach (ref readonly var submesh in mesh.MeshData->Submeshes.AsReadOnlySpan())
            {
                commandList.DrawIndexedInstanced(submesh.IndexCount, 1, submesh.StartIndexLocation, 0);
            }
        }
    }

    /// <summary>
    /// Ends the pass and closes the command list.
    /// </summary>
    [System]
    public static void EndPass(in GBufferRenderPass pass, in RenderGraph graph)
    {
        graph.End(pass.PassHandle);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(GBufferRenderPass* pass, in RenderGraph graph)
    {
        Logger.Warning<GBufferRenderPass>("Shutdown has not been implemented");
        graph.DestroyPass(pass->PassHandle);
        pass->PassHandle = Handle<RenderPass>.Invalid;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct GBufferPassData
    {
        public int MeshIndex;
    }
}

