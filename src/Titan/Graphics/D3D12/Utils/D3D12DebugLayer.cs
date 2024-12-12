using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Systems;
using static Titan.Platform.Win32.D3D12.D3D12Common;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Graphics.D3D12.Utils;

internal unsafe partial struct D3D12DebugLayer
{
    private static ComPtr<ID3D12Debug1> D3D12Debug;
    private static ComPtr<IDXGIDebug> DXGIDebug;
    private static ComPtr<IDXGIInfoQueue> DXGIInfoQueue;

    private static TitanBuffer MessageBuffer;

    private static readonly Guid Producer = *DXGI_DEBUG_ID.IID_DXGI_DEBUG_ALL;

    [System(SystemStage.Startup, SystemExecutionType.Inline)]
    public static void Init()
    {
        const bool GPUValidation = false; // make this configurable when we need it.

        var hr = D3D12GetDebugInterface(ID3D12Debug1.Guid, (void**)D3D12Debug.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to get the {nameof(ID3D12Debug1)} interface. HRESULT = {hr}");
            return;
        }

        D3D12Debug.Get()->EnableDebugLayer();
        D3D12Debug.Get()->SetEnableGPUBasedValidation(GPUValidation);

        hr = DXGICommon.DXGIGetDebugInterface(IDXGIDebug.Guid, (void**)DXGIDebug.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to query the interface {nameof(IDXGIDebug)}. HRESULT = {hr}");
            return;
        }

        hr = DXGICommon.DXGIGetDebugInterface(IDXGIInfoQueue.Guid, (void**)DXGIInfoQueue.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12DebugLayer>($"Failed to get the {nameof(DXGIInfoQueue)} interface. HRESULT = {hr}");
        }

        var bufferSize = MemoryUtils.MegaBytes(1);
        var buffer = MemoryUtils.GlobalAlloc(bufferSize);

        Debug.Assert(buffer != null);
        MessageBuffer = new((char*)buffer, bufferSize);

        Logger.Info<D3D12DebugLayer>($"Successfully initialize the D3D12 Debug Layer. GPUValidation = {GPUValidation}");
    }

    [System]
    public static void GetDebugMessages()
    {
        if (!DXGIInfoQueue.IsValid)
        {
            return;
        }

        if (Debugger.IsAttached)
        {
            return;
        }
        var numberOfMessages = DXGIInfoQueue.Get()->GetNumStoredMessages(Producer);
        for (var i = 0ul; i < numberOfMessages; ++i)
        {
            var message = (DXGI_INFO_QUEUE_MESSAGE*)MessageBuffer.AsPointer();
            nuint messageLength;
            var hr = DXGIInfoQueue.Get()->GetMessage(Producer, i, message, &messageLength);
            if (FAILED(hr))
            {
                Logger.Error<D3D12DebugLayer>($"Failed to get the message at index {i}. HRESULT = {hr}");
                continue;
            }

            var description = Encoding.ASCII.GetString(message->pDescription, (int)messageLength);
            Logger.Error<D3D12DebugLayer>(description);
        }

        DXGIInfoQueue.Get()->ClearStoredMessages(Producer);
    }

    [System(SystemStage.EndOfLife)]
    public static void Shutdown()
    {
        if (DXGIDebug.IsValid)
        {
            DXGIDebug.Get()->ReportLiveObjects(IID.DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_ALL);
        }

        DXGIDebug.Dispose();
        D3D12Debug.Dispose();

        if (MessageBuffer.IsValid)
        {
            MemoryUtils.GlobalFree(MessageBuffer.AsPointer());
            MessageBuffer = default;
        }
    }
}
