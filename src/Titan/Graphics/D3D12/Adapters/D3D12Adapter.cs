using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Graphics.D3D12.Adapters;

[UnmanagedResource]
internal unsafe partial struct D3D12Adapter
{
    public const int MaxAdapters = 10;
    public uint PrimaryAdapterIndex;
    public uint AdapterCount;
    public Inline8<AdapterInfo> Adapters;
    public readonly ref readonly AdapterInfo PrimaryAdapter => ref Adapters[PrimaryAdapterIndex];


    [System(SystemStage.PreInit)]
    public static void Init(D3D12Adapter* adapter, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<RenderingConfig>();

        var flags = config.Debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;
        Logger.Trace<D3D12Adapter>($"Creating {nameof(IDXGIFactory7)}. Flags = {flags}");
        using ComPtr<IDXGIFactory7> factory = default;
        var hr = DXGICommon.CreateDXGIFactory2(flags, factory.UUID, (void**)factory.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12Adapter>($"Failed to create a {nameof(IDXGIFactory7)}. HRESULT = {hr}");
            //TODO(Jens): Implement fatal error
            return;
        }

        //NOTE(Jens): We use the first device we find if there's no stored config. We enumerate them by GPU preference so we expect to get the best one first.
        adapter->PrimaryAdapterIndex = 0;

        for (var index = 0u; index < MaxAdapters; ++index)
        {
            IDXGIAdapter3* dxgiAdapter;
            hr = factory.Get()->EnumAdapterByGpuPreference(index, DXGI_GPU_PREFERENCE.DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE, IDXGIAdapter3.Guid, (void**)&dxgiAdapter);
            if (hr == DXGI_ERROR.DXGI_ERROR_NOT_FOUND)
            {
                break;
            }
            if (FAILED(hr))
            {
                Logger.Error<D3D12Adapter>($"Failed to enumerate adapters. Index = {index} HRESULT = {hr}");
                break;
            }

            DXGI_ADAPTER_DESC1 desc;
            hr = dxgiAdapter->GetDesc1(&desc);
            if (FAILED(hr))
            {
                Logger.Error<D3D12Adapter>($"Failed to get the adapter desc. Index = {index} HRESULT = {hr}");
                dxgiAdapter->Release();
                break;
            }

            ref var info = ref adapter->Adapters[adapter->AdapterCount++];
            info.Adapter = dxgiAdapter;
            info.SetName(desc.DescriptionString());
            info.DeviceId = desc.DeviceId;
            info.VendorId = desc.VendorId;
            info.IsHardware = (desc.Flags & DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) == 0;

            Logger.Trace<D3D12Adapter>($"Found adapter {info.DebugString}");
        }

        if (config.Adapter != null)
        {
            var deviceId = config.Adapter.DeviceId;
            var vendorId = config.Adapter.VendorId;
            Logger.Info<D3D12Adapter>($"Using configured adapter. DeviceId = {deviceId} VendorId = {vendorId}");
            var found = false;
            for (var i = 0u; i < adapter->AdapterCount; ++i)
            {
                ref readonly var info = ref adapter->Adapters[i];
                if (info.DeviceId == deviceId && info.VendorId == vendorId)
                {
                    adapter->PrimaryAdapterIndex = i;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Logger.Warning<D3D12Adapter>($"The adapter in the configuration could not be found, using default device. Configured DeviceId = {deviceId} Configured VendorId = {vendorId}");
            }
        }

        Logger.Trace<D3D12Adapter>($"Found {adapter->AdapterCount} adapters.");
    }

    [System(SystemStage.PostShutdown)]
    public static void Shutdown(D3D12Adapter* adapter)
    {
        for (var i = 0; i < adapter->AdapterCount; ++i)
        {
            adapter->Adapters[i].Adapter.Dispose();
        }

        *adapter = default;
    }
}
