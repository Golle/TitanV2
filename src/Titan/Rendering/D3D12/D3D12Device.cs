using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D;
using Titan.Platform.Win32.D3D12;
using Titan.Rendering.D3D12.Adapters;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12;
internal sealed unsafe class D3D12Device : IService
{
    private ComPtr<ID3D12Device4> _device;

    internal ID3D12Device4* Device => _device;
    public bool Init(D3D12Adapter adapter, D3D_FEATURE_LEVEL featureLevel)
    {
        var hr = D3D12Common.D3D12CreateDevice(adapter.PrimaryAdapter, featureLevel, _device.UUID, (void**)_device.GetAddressOf());

        if (FAILED(hr))
        {
            Logger.Error<D3D12Device>($"Failed to create the {nameof(ID3D12Device4)}. HRESULT = {hr}");
            return false;
        }


        return true;
    }


    public void Shutdown()
    {
        _device.Dispose();
    }
}
