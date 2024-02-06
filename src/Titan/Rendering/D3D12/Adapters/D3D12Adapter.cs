using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12.Adapters;
internal sealed unsafe class D3D12Adapter : IService
{
    private TitanArray<DXGIAdapter> _adapters;
    private IMemoryManager? _memoryManager;

    private uint _primaryAdapterIndex = 0;
    public ref readonly DXGIAdapter PrimaryAdapter => ref _adapters[_primaryAdapterIndex];
    public bool Init(IMemoryManager memoryManager, AdapterConfig? config, bool debug)
    {
        var flags = debug ? DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG : 0;
        Logger.Trace<D3D12Adapter>($"Creating {nameof(IDXGIFactory7)}. Flags = {flags}");
        using ComPtr<IDXGIFactory7> factory = default;
        var hr = DXGICommon.CreateDXGIFactory2(flags, factory.UUID, (void**)factory.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<D3D12Adapter>($"Failed to create a {nameof(IDXGIFactory7)}. HRESULT = {hr}");
            return false;
        }

        const int maxAdapters = 10;
        Span<DXGIAdapter> adapters = stackalloc DXGIAdapter[maxAdapters];
        adapters.Clear();

        uint index;
        for (index = 0u; index < maxAdapters; ++index)
        {
            IDXGIAdapter3* adapter = null;
            hr = factory.Get()->EnumAdapterByGpuPreference(index, DXGI_GPU_PREFERENCE.DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE, IDXGIAdapter3.Guid, (void**)&adapter);
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
            hr = adapter->GetDesc1(&desc);
            if (FAILED(hr))
            {
                Logger.Error<D3D12Adapter>($"Failed to get the adapter desc. Index = {index} HRESULT = {hr}");
                break;
            }

            adapters[(int)index] = new(adapter, desc);
            Logger.Trace<D3D12Adapter>($"Found adapter {adapters[(int)index].DebugString}");
        }

        if (index == 0)
        {
            Logger.Error<D3D12Adapter>("No adapters found.");
            return false;
        }

        if (!memoryManager.TryAllocArray(out _adapters, index))
        {
            Logger.Error<D3D12Adapter>($"Failed to allocate array. Count = {index} Size = {sizeof(DXGIAdapter) * index}");
            return false;
        }

        adapters[..(int)index]
            .CopyTo(_adapters.AsSpan());

        _primaryAdapterIndex = GetPrimaryAdapterIndex(config);

        _memoryManager = memoryManager;

        return true;
    }

    private uint GetPrimaryAdapterIndex(AdapterConfig? config)
    {
        if (config == null)
        {
            Logger.Trace<D3D12Adapter>("No adapter configuration");
            return 0;
        }

        for (var i = 0u; i < _adapters.Length; ++i)
        {
            ref readonly var desc = ref _adapters[i].Desc;
            if (desc.DeviceId == config.DeviceId && desc.VendorId == config.VendorId)
            {
                Logger.Trace<D3D12Adapter>($"Found a matching Adapter. Name = {_adapters[i].Name} DeviceId = {config.DeviceId} VendorId = {config.VendorId}");
                return i;
            }
        }

        Logger.Trace<D3D12Adapter>($"No matching adapter found. DeviceId = {config.DeviceId} VendorId = {config.VendorId}");
        return 0;
    }

    public void Shutdown()
    {
        foreach (ref var adapter in _adapters.AsSpan())
        {
            adapter.Dispose();
        }
        _memoryManager?.FreeArray(ref _adapters);
    }
}
