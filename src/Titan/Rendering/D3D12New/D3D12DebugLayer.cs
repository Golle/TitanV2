using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12New;

[UnmanagedResource]
internal unsafe partial struct D3D12DebugLayer
{
    public ComPtr<ID3D12Debug1> D3D12Debug;
    public ComPtr<IDXGIDebug> DXGIDebug;

    [System(SystemStage.Init)]
    public static void Init(D3D12DebugLayer* layer)
    {
        const bool GPUValidation = false; // make this configurable when we need it.

        var hr = D3D12GetDebugInterface(ID3D12Debug1.Guid, (void**)layer->D3D12Debug.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to get the {nameof(ID3D12Debug1)} interface. HRESULT = {hr}");
            return;
        }

        layer->D3D12Debug.Get()->EnableDebugLayer();
        layer->D3D12Debug.Get()->SetEnableGPUBasedValidation(GPUValidation);

        hr = DXGICommon.DXGIGetDebugInterface(IDXGIDebug.Guid, (void**)layer->DXGIDebug.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to query the interface {nameof(IDXGIDebug)}. HRESULT = {hr}");
            return;
        }
        Logger.Info<D3D12DebugLayer>($"Successfully initialize the D3D12 Debug Layer. GPUValidation = {GPUValidation}");
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(D3D12DebugLayer* layer)
    {
        if (layer->DXGIDebug.IsValid)
        {
            layer->DXGIDebug.Get()->ReportLiveObjects(IID.DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_ALL);
        }

        layer->DXGIDebug.Dispose();
        layer->D3D12Debug.Dispose();
    }
}
