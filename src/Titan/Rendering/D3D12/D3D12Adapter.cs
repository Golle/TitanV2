using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.D3D12;
internal sealed unsafe class D3D12Adapter : IService
{
    private TitanArray<DXGIAdapter> _adapters;
    private IMemorySystem? _memorySystem;
    public bool Init(IMemorySystem memorySystem, bool debug)
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

        var count = index - 1;
        if (!memorySystem.TryAllocArray(out _adapters, count))
        {
            Logger.Error<D3D12Adapter>($"Failed to allocate array. Count = {count} Size = {sizeof(DXGIAdapter) * index}");
            return false;
        }

        Debug.Assert(count < maxAdapters);
        adapters[..(int)count]
            .CopyTo(_adapters.AsSpan());

        _memorySystem = memorySystem;

        return true;
    }

    public void Shutdown()
    {
        foreach (ref var adapter in _adapters.AsSpan())
        {
            adapter.Dispose();
        }
        _memorySystem?.FreeArray(ref _adapters);
    }
}

internal unsafe struct DXGIAdapter(IDXGIAdapter3* adapter, in DXGI_ADAPTER_DESC1 desc) : IDisposable
{
    private ComPtr<IDXGIAdapter3> _adapter = adapter;
    private readonly DXGI_ADAPTER_DESC1 _desc = desc;

    [UnscopedRef]
    public ref readonly DXGI_ADAPTER_DESC1 Desc => ref _desc;
    public ReadOnlySpan<char> Name => _desc.DescriptionString();
    public bool IsHardware => !IsSoftware;
    public bool IsSoftware => (_desc.Flags & DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) != 0;
    public string DebugString => $"{Name} Hardware = {IsHardware}";

    public void Dispose() => _adapter.Dispose();

    public static implicit operator IDXGIAdapter3*(in DXGIAdapter adapter) => adapter._adapter;
    public static implicit operator IUnknown*(in DXGIAdapter adapter) => (IUnknown*)adapter._adapter.Get();
}
