using Titan.Core.Logging;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Platform.Win32;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;
using Titan.Application.Services;

namespace Titan.Rendering.D3D12.Utils;
internal sealed unsafe class D3D12DebugLayer : IService
{
    private ComPtr<ID3D12Debug1> _d3d12Debug;
    private ComPtr<IDXGIDebug> _dxgiDebug;

    public bool Init(bool gpuBasedValidation = true)
    {
        ID3D12Debug1* d3d12Debug;
        var hr = D3D12GetDebugInterface(ID3D12Debug1.Guid, (void**)&d3d12Debug);
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to get the {nameof(ID3D12Debug1)} interface. HRESULT = {hr}");
            return false;
        }

        d3d12Debug->EnableDebugLayer();
        d3d12Debug->SetEnableGPUBasedValidation(gpuBasedValidation);

        IDXGIDebug* dxgiDebug;
        hr = DXGICommon.DXGIGetDebugInterface(IDXGIDebug.Guid, (void**)&dxgiDebug);
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to query the interface {nameof(IDXGIDebug)}. HRESULT = {hr}");
            return false;
        }

        _d3d12Debug = d3d12Debug;
        _dxgiDebug = dxgiDebug;
        return true;
    }

    public void ReportLiveObjects()
    {
        if (_dxgiDebug.IsValid)
        {
            _dxgiDebug.Get()->ReportLiveObjects(IID.DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_ALL);
        }
    }

    public void Shutdown()
    {
        _d3d12Debug.Dispose();
        _dxgiDebug.Dispose();
    }
}
