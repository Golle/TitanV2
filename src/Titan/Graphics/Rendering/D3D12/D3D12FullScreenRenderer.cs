using Titan.Assets;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Graphics.Resources;
using Titan.Platform.Win32;
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
    public Color ClearColor;

    [System(SystemStage.Init)]
    public static void Init(in D3D12Device device, D3D12FullScreenRenderer* data, IConfigurationManager configurationManager, IAssetsManager assetsManager)
    {
        var config = configurationManager.GetConfigOrDefault<D3D12Config>();

        var pixelShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimplePixelShader);
        var vertexShaderHandle = assetsManager.LoadImmediately<ShaderAsset>(EngineAssetsRegistry.SimpleVertexShader);

        var pixelShader = assetsManager.Get(pixelShaderHandle).ShaderByteCode;
        var vertexShader = assetsManager.Get(vertexShaderHandle).ShaderByteCode;

        ReadOnlySpan<D3D12_ROOT_PARAMETER1> rootParameters = [];
        ReadOnlySpan<D3D12_STATIC_SAMPLER_DESC> samplers = [
            D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 0, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL),
            D3D12Helpers.CreateStaticSamplerDesc(SamplerState.Linear, 1, 0, D3D12_SHADER_VISIBILITY.D3D12_SHADER_VISIBILITY_PIXEL)
        ];

        //TODO(Jens): IMPLEMENT THE UPLOAD QUEUE, and maybe a generic resource handling system :| 

        var rootSignature = device.CreateRootSignature(D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_NONE, rootParameters, samplers);
        if (rootSignature == null)
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
            .RootSignature(rootSignature)
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
    public static void Render(in D3D12CommandQueue queue, in D3D12FullScreenRenderer data, in DXGISwapchain swapchain, in Window window)
    {
        var commandList = queue.GetCommandList(data.PipelineState.Get());
        var backbuffer = swapchain.CurrentBackbuffer;
        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);

        commandList.SetRenderTarget(backbuffer);
        commandList.ClearRenderTargetView(backbuffer, MemoryUtils.AsPointer(data.ClearColor));

        commandList.SetIndexBuffer();

        commandList.DrawIndexedInstanced(4, 1);

        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        commandList.Close();
    }
}
