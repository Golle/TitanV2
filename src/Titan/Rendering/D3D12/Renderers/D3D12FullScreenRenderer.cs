using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Utils;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering.Resources;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.D3D12.Renderers;

[StructLayout(LayoutKind.Sequential, Size = 256)]
public struct TestData
{
    public Color Color;
    public Matrix4x4 ViewProjectionMatrix;
    public int TextureIndex;
    public float Time;
}

[UnmanagedResource]
internal unsafe partial struct D3D12FullScreenRenderer
{
    public ComPtr<ID3D12PipelineState> PipelineState;
    public ComPtr<ID3D12PipelineState> PipelineStateWireframe;
    public ComPtr<ID3D12RootSignature> RootSignature;
    public ComPtr<ID3D12Resource> ConstantBuffer;
    public void* ConstantBufferMap;
    public Color ClearColor;

    public ComPtr<ID3D12Resource> VertexBuffer;
    public Handle<Texture> DepthBuffer;
    public ComPtr<ID3D12Resource> IndexBuffer;
    public D3D12_INDEX_BUFFER_VIEW IndexBufferView;
    public Handle<Texture> Texture;
    public AssetHandle<MeshAsset> Mesh;
    public uint Count;

    public Vector3 CameraPosition;

    [System(SystemStage.Init)]
    public static void Init(in D3D12Device device, D3D12FullScreenRenderer* data, IConfigurationManager configurationManager, AssetsManager assetsManager, in D3D12Allocator allocator, in Window window, in D3D12ResourceManager resourceManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();

        var pixelShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimplePixelShader);
        var vertexShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimpleVertexShader);

        var assetHandle = assetsManager.LoadImmediately<TextureAsset>(EngineAssetsRegistry.BookTexture);
        data->Texture = assetsManager.Get(assetHandle).Handle;

        var meshHandle = assetsManager.LoadImmediately<MeshAsset>(EngineAssetsRegistry.Book);

        data->Mesh = meshHandle;
        ref readonly var mesh = ref assetsManager.Get(meshHandle);
        //data->VertexBuffer = resourceManager.Access(mesh.VertexBuffer)->Resource;
        //data->IndexBuffer = resourceManager.Access(mesh.IndexBuffer)->Resource;
        //data->Count = mesh.IndexCount;

        //var cbSize = (uint)sizeof(TestData);
        //data->ConstantBuffer = device.CreateBuffer(cbSize, true, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);

        //D3D12_INDEX_BUFFER_VIEW indexBufferView = new()
        //{
        //    BufferLocation = data->IndexBuffer.Get()->GetGPUVirtualAddress(),
        //    Format = DXGI_FORMAT.DXGI_FORMAT_R32_UINT,
        //    SizeInBytes = sizeof(uint) * mesh.IndexCount
        //};
        //data->IndexBufferView = indexBufferView;


        //// Constant buffer
        //var srv = allocator.Allocate(DescriptorHeapType.ShaderResourceView);
        //device.CreateConstantBufferView(cbSize, data->ConstantBuffer.Get()->GetGPUVirtualAddress(), srv.CPU);
        //data->ConstantBuffer.Get()->Map(0, null, &data->ConstantBufferMap);

        //// Vertex buffer (any mesh really)
        //var srv1 = allocator.Allocate(DescriptorHeapType.ShaderResourceView);
        //device.CreateShaderResourceView1(data->VertexBuffer, srv1.CPU, (uint)mesh.IndexCount, (uint)sizeof(Vertex));

        data->DepthBuffer = resourceManager.CreateDepthBuffer(new CreateDepthBufferArgs
        {
            Format = DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT,
            Height = (uint)window.Height,
            Width = (uint)window.Width,
            ClearValue = 1.0f
        });

        var pixelShader = assetsManager.Get(pixelShaderHandle).ShaderByteCode;
        var vertexShader = assetsManager.Get(vertexShaderHandle).ShaderByteCode;

        var ranges = stackalloc D3D12_DESCRIPTOR_RANGE1[6];
        D3D12Helpers.InitDescriptorRanges(new Span<D3D12_DESCRIPTOR_RANGE1>(ranges, 6), D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV);

        ReadOnlySpan<D3D12_ROOT_PARAMETER1> rootParameters = [
            CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(6, ranges),
            CD3DX12_ROOT_PARAMETER1.AsConstantBufferView(0 , 0,D3D12_ROOT_DESCRIPTOR_FLAGS.D3D12_ROOT_DESCRIPTOR_FLAG_DATA_STATIC),
        ];
        ReadOnlySpan<D3D12_STATIC_SAMPLER_DESC> samplers = [
            D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 0, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL),
            D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 1, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL)
        ];

        data->RootSignature = device.CreateRootSignature(D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_NONE, rootParameters, samplers);
        if (!data->RootSignature.IsValid)
        {
            Logger.Error<D3D12FullScreenRenderer>("Failed to create the Root Signature.");
        }


        D3D12_DEPTH_STENCIL_DESC depthStencilDesc = new()
        {
            DepthEnable = 1,
            DepthWriteMask = D3D12_DEPTH_WRITE_MASK.D3D12_DEPTH_WRITE_MASK_ALL,
            DepthFunc = D3D12_COMPARISON_FUNC.D3D12_COMPARISON_FUNC_LESS,
            StencilEnable = 0
        };
        D3D12_RT_FORMAT_ARRAY renderTargets = new(DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM);

        {
            var stream = new D3D12PipelineSubobjectStream()
                .Blend(D3D12Helpers.GetBlendState(BlendStateType.AlphaBlend))
                .DepthStencil(depthStencilDesc)
                .DepthStencilfFormat(DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT)
                .PS(new D3D12_SHADER_BYTECODE
                {
                    pShaderBytecode = pixelShader.AsPointer(),
                    BytecodeLength = pixelShader.Size
                })
                .VS(new D3D12_SHADER_BYTECODE
                {
                    pShaderBytecode = vertexShader.AsPointer(),
                    BytecodeLength = vertexShader.Size
                })
                .Razterizer(D3D12_RASTERIZER_DESC.Default() with
                {
                    CullMode = D3D12_CULL_MODE.D3D12_CULL_MODE_BACK
                })
                .RenderTargetFormat(renderTargets)
                .RootSignature(data->RootSignature)
                .Sample(new DXGI_SAMPLE_DESC
                {
                    Count = 1,
                    Quality = 0
                })
                .SampleMask(uint.MaxValue)
                .Topology(D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE)
                .AsStreamDesc();


            data->PipelineState = device.CreatePipelineStateObject(stream);
        }

        {
            var stream = new D3D12PipelineSubobjectStream()
                .Blend(D3D12Helpers.GetBlendState(BlendStateType.AlphaBlend))
                .DepthStencil(depthStencilDesc)
                .DepthStencilfFormat(DXGI_FORMAT.DXGI_FORMAT_D32_FLOAT)
                .PS(new D3D12_SHADER_BYTECODE
                {
                    pShaderBytecode = pixelShader.AsPointer(),
                    BytecodeLength = pixelShader.Size
                })
                .VS(new D3D12_SHADER_BYTECODE
                {
                    pShaderBytecode = vertexShader.AsPointer(),
                    BytecodeLength = vertexShader.Size
                })
                .Razterizer(D3D12_RASTERIZER_DESC.Default() with
                {
                    CullMode = D3D12_CULL_MODE.D3D12_CULL_MODE_NONE,
                    FillMode = D3D12_FILL_MODE.D3D12_FILL_MODE_WIREFRAME
                })
                .RenderTargetFormat(renderTargets)
                .RootSignature(data->RootSignature)
                .Sample(new DXGI_SAMPLE_DESC
                {
                    Count = 1,
                    Quality = 0
                })
                .SampleMask(uint.MaxValue)
                .Topology(D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE)
                .AsStreamDesc();


            data->PipelineStateWireframe = device.CreatePipelineStateObject(stream);

        }
        data->ClearColor = config.ClearColor;

        if (!data->PipelineState.IsValid)
        {
            Logger.Error<D3D12FullScreenRenderer>("Failed to init the pipeline state.");
        }

        Logger.Trace<D3D12FullScreenRenderer>($"Loaded shaders: PixelShader = {pixelShader.IsValid} VertexShader = {vertexShader.IsValid}");

        data->CameraPosition = Vector3.UnitZ * -10;
    }

    //[System]
    public static void Render(in D3D12CommandQueue queue, ref D3D12FullScreenRenderer data, in DXGISwapchain swapchain, in Window window, in D3D12Allocator allocator, in D3D12ResourceManager resourceManager, AssetsManager assetsManager, in InputState inputState)
    {
        var pipelineState = inputState.IsKeyDown(KeyCode.Space) ? data.PipelineStateWireframe : data.PipelineState;

        Debug.Fail("Not working anymore.");
        var commandList = queue.GetCommandList(pipelineState);
        var backbuffer = resourceManager.Access(swapchain.CurrentBackbuffer);
        var depthBuffer = resourceManager.Access(data.DepthBuffer);

        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);

        commandList.SetTopology(D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        commandList.SetGraphicsRootSignature(data.RootSignature);
        commandList.SetRenderTarget(backbuffer, depthBuffer);
        commandList.SetDescriptorHeap(allocator.SRV.Heap);
        commandList.SetGraphicsRootDescriptorTable(0, allocator.SRV.GPUStart);

        commandList.SetGraphicsRootConstantBufferView(1, data.ConstantBuffer.Get()->GetGPUVirtualAddress());

        commandList.ClearRenderTargetView(backbuffer, MemoryUtils.AsPointer(data.ClearColor));
        commandList.ClearDepthStencilView(depthBuffer, D3D12_CLEAR_FLAGS.D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, null);

        //commandList.SetGraphicsRootShaderResourceView(2, data.VertexBufferView.BufferLocation);

        D3D12_VIEWPORT viewport = new()
        {
            Width = window.Width,
            Height = window.Height,
            MaxDepth = 1f,
            MinDepth = 0f,
            TopLeftX = 0,
            TopLeftY = 0
        };

        D3D12_RECT rect = new()
        {
            Bottom = window.Height,
            Left = 0,
            Right = window.Width,
            Top = 0
        };

        // Define camera parameters
        //Vector3 cameraPosition = new Vector3(0, 0, -10); // Position of the camera
        Vector3 target = Vector3.Zero; // Target the camera is looking at
        Vector3 up = Vector3.UnitY; // Up direction for the camera
        Vector3 forward = Vector3.UnitZ;
        Vector3 right = Vector3.UnitX;

        // Define projection parameters
        float fov = MathF.PI / 4; // Field of view (in radians)
        float aspectRatio = window.Width / (float)window.Height; // Aspect ratio (width / height)
        float nearPlane = 0.1f; // Near plane distance
        float farPlane = 1000f; // Far plane distance

        var worldMatrix = Matrix4x4.Identity;
        // Create projection matrix
        var projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        // Create view matrix
        var viewMatrix = Matrix4x4.CreateLookAt(data.CameraPosition, target, up);

        // Combine view and projection matrices to get view-projection matrix
        var viewProjectionMatrix = worldMatrix * viewMatrix * projectionMatrix;
        if (inputState.IsKeyDown(KeyCode.Down) || inputState.IsKeyDown(KeyCode.S))
        {
            data.CameraPosition -= forward * 0.1f;
        }
        if (inputState.IsKeyDown(KeyCode.Up) || inputState.IsKeyDown(KeyCode.W))
        {
            data.CameraPosition += forward * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.Left) || inputState.IsKeyDown(KeyCode.A))
        {
            data.CameraPosition -= right * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.Right) || inputState.IsKeyDown(KeyCode.D))
        {
            data.CameraPosition += right * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.V))
        {
            data.CameraPosition += up * 0.1f;
        }

        if (inputState.IsKeyDown(KeyCode.C))
        {
            data.CameraPosition -= up * 0.1f;
        }

        var tex = resourceManager.Access(data.Texture);
        var d = (TestData*)data.ConstantBufferMap;
        d->Color = Color.White;
        d->TextureIndex = tex->SRV.Index;
        d->ViewProjectionMatrix = Matrix4x4.Transpose(viewProjectionMatrix);
        d->Time += 0.0003f;
        commandList.SetViewport(&viewport);
        commandList.SetScissorRect(&rect);
        //commandList.SetIndexBuffer();
        commandList.SetIndexBuffer(data.IndexBufferView);
        //commandList.DrawInstanced();
        var mesh = assetsManager.Get(data.Mesh);
        //for (var i = 0; i < mesh.SubMeshCount; ++i)
        //{
        //    commandList.DrawIndexedInstanced((uint)mesh.SubMeshes[i].VertexCount, 1, (uint)mesh.SubMeshes[i].VertexOffset, mesh.SubMeshes[i].VertexOffset, 0);
        //    //commandList.DrawIndexedInstanced(3/*(uint)mesh.SubMeshes[i].VertexCount*/, 1, 0, 0, 0);
        //}
        ////commandList.DrawInstanced(, 1);

        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        commandList.Close();
    }
}
