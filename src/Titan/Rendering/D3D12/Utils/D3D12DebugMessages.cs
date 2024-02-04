using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Application.Services;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;

namespace Titan.Rendering.D3D12.Utils;

internal sealed unsafe class D3D12DebugMessages : IService
{
    private ComPtr<ID3D12InfoQueue1> _infoQueue;

    public bool Init(D3D12Device device)
    {
        Logger.Warning<D3D12DebugMessages>($"The interface {nameof(ID3D12InfoQueue1)} is not available in Windows 10, only in the SDK which we can't load.");
        var hr = device.Device->QueryInterface(_infoQueue.UUID, (void**)_infoQueue.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Warning<D3D12DebugMessages>($"Failed to get the interface {nameof(ID3D12InfoQueue1)}. HRESULT = {hr}");
            return false;
        }

        uint callbackCookie;
        hr = _infoQueue.Get()->RegisterMessageCallback(&OnMessage, D3D12_MESSAGE_CALLBACK_FLAGS.D3D12_MESSAGE_CALLBACK_IGNORE_FILTERS, null, &callbackCookie);
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<D3D12DebugMessages>($"Failed to register message callback in {nameof(ID3D12InfoQueue1)}. HRESULT = {hr}");

            _infoQueue.Dispose();
            return false;
        }

        Debug.Fail($"For some reason this magically works now! Implement support for {nameof(ID3D12InfoQueue1)}");
        return true;
    }

    [UnmanagedCallersOnly]
    private static void OnMessage(D3D12_MESSAGE_CATEGORY category, D3D12_MESSAGE_SEVERITY severity, D3D12_MESSAGE_ID id, byte* wtf, void* context)
    {

    }

    public void Shutdown()
    {
        _infoQueue.Dispose();
    }
}
