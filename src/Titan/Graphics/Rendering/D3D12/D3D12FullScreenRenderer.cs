using Titan.Assets;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Memory;
using Titan.Graphics.D3D12.Utils;
using Titan.Graphics.Resources;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Graphics.Rendering.D3D12;

[UnmanagedResource]
internal unsafe partial struct D3D12FullScreenRenderer
{
    public ComPtr<ID3D12PipelineState> PipelineState;
    public ComPtr<ID3D12RootSignature> RootSignature;
    
    public Color ClearColor;

    public D3D12Texture2D Texture;

    [System(SystemStage.Init)]
    public static void Init(in D3D12Device device, D3D12FullScreenRenderer* data, IConfigurationManager configurationManager, IAssetsManager assetsManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();

        var pixelShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimplePixelShader);
        var vertexShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimpleVertexShader);

        var textureHandle = assetsManager.LoadImmediately<TextureAsset>(EngineAssetsRegistry.UnnamedAsset0);
        data->Texture = assetsManager.Get(textureHandle).D3D12Texture2D;

        var pixelShader = assetsManager.Get(pixelShaderHandle).ShaderByteCode;
        var vertexShader = assetsManager.Get(vertexShaderHandle).ShaderByteCode;

        var ranges = stackalloc D3D12_DESCRIPTOR_RANGE1[6];
        D3D12Helpers.InitDescriptorRanges(new Span<D3D12_DESCRIPTOR_RANGE1>(ranges, 6), D3D12_DESCRIPTOR_RANGE_TYPE.D3D12_DESCRIPTOR_RANGE_TYPE_SRV);

        ReadOnlySpan<D3D12_ROOT_PARAMETER1> rootParameters = [
            CD3DX12_ROOT_PARAMETER1.AsDescriptorTable(6, ranges),
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

        D3D12_RT_FORMAT_ARRAY renderTargets;
        renderTargets.RTFormats[0] = (int)DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;
        renderTargets.NumRenderTargets = 1;
        var stream = new D3D12PipelineSubobjectStream()
            .Blend(D3D12Helpers.GetBlendState(BlendStateType.Disabled))
            //.DepthStencil(default)
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
            .Razterizer(D3D12_RASTERIZER_DESC.Default())
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
        data->ClearColor = config.ClearColor;

        if (!data->PipelineState.IsValid)
        {
            Logger.Error<D3D12FullScreenRenderer>("Failed to init the pipeline state.");
        }

        Logger.Trace<D3D12FullScreenRenderer>($"Loaded shaders: PixelShader = {pixelShader.IsValid} VertexShader = {vertexShader.IsValid}");
    }

    [System]
    public static void Render(in D3D12CommandQueue queue, in D3D12FullScreenRenderer data, in DXGISwapchain swapchain, in Window window, in D3D12Allocator allocator)
    {
        var commandList = queue.GetCommandList(data.PipelineState.Get());
        var backbuffer = swapchain.CurrentBackbuffer;
        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);

        commandList.SetTopology(D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        commandList.SetGraphicsRootSignature(data.RootSignature);
        commandList.SetRenderTarget(backbuffer);
        commandList.ClearRenderTargetView(backbuffer, MemoryUtils.AsPointer(data.ClearColor));
        commandList.SetDescriptorHeap(allocator.SRV.Heap);
        commandList.SetGraphicsRootDescriptorTable(0, allocator.SRV.GPUStart);
        var i = data.Texture.SRV.Index;
        D3D12_VIEWPORT viewport = new()
        {
            Width = window.Width,
            Height = window.Height,
            MaxDepth = 1,
            MinDepth = -1,
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

        commandList.SetViewport(&viewport);
        commandList.SetScissorRect(&rect);
        //commandList.SetIndexBuffer();

        commandList.DrawInstanced(3, 1);

        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        commandList.Close();
    }
}
