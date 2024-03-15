using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Graphics.D3D12;
using Titan.Graphics.D3D12.Utils;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.D3D12;

[UnmanagedResource]
internal unsafe partial struct D3D12FullScreenRenderer
{

    public ComPtr<ID3D12PipelineState> PipelineState;

    [System(SystemStage.Init)]
    public static void Init(in D3D12Device device, D3D12FullScreenRenderer* data)
    {

        var stream = new D3D12PipelineSubobjectStream()
            //.Blend(new D3D12_BLEND_DESC
            //{
                 
            //})
            //.DepthStencil(default)
            .PS(default)
            .VS(default)
            .Razterizer(default)
            .RenderTargetFormat(default)
            .RootSignature(default)
            .Sample(default)
            .SampleMask(default)
            .Topology(default)
            .AsStreamDesc();


        data->PipelineState = device.CreatePipelineStateObject(stream);
        if (!data->PipelineState.IsValid)
        {
            Logger.Error<D3D12FullScreenRenderer>("Failed to init the pipeline state.");
        }
    }

    [System]
    public static void Render(in D3D12CommandQueue queue, in D3D12FullScreenRenderer data, in DXGISwapchain swapchain, in Window window)
    {
        var commandList = queue.GetCommandList(data.PipelineState.Get());
        var backbuffer = swapchain.CurrentBackbuffer;
        var color = new Color(0.12f, 1.2f, 0.3f);
        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);
        
        commandList.SetRenderTarget(backbuffer);
        commandList.ClearRenderTargetView(backbuffer, &color);

        
        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        commandList.Close();
    }


}
