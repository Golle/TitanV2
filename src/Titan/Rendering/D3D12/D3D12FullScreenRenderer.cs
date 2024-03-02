using Titan.Core.Maths;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.D3D12;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering.D3D12;
internal unsafe partial struct D3D12FullScreenRenderer
{
    [System]
    public static void Render(in D3D12CommandQueue queue, in DXGISwapchain swapchain, in Window window)
    {
        var commandList = queue.GetCommandList(null);
        var backbuffer = swapchain.CurrentBackbuffer;
        var color = new Color(0.12f, 1.2f, 0.3f);

        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET);
        commandList.SetRenderTarget(backbuffer);
        commandList.ClearRenderTargetView(backbuffer, &color);
        commandList.Transition(backbuffer, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATES.D3D12_RESOURCE_STATE_PRESENT);
        commandList.Close();
    }
}
