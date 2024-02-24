using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;

namespace Titan.Rendering.D3D12.Adapters;

[DebuggerDisplay("{DebugString,nb}")]
internal unsafe struct DXGIAdapter(IDXGIAdapter3* adapter, in DXGI_ADAPTER_DESC1 desc) : IDisposable
{
    private ComPtr<IDXGIAdapter3> _adapter = adapter;
    private readonly DXGI_ADAPTER_DESC1 _desc = desc;

    [UnscopedRef]
    public ref readonly DXGI_ADAPTER_DESC1 Desc => ref _desc;
    public ReadOnlySpan<char> Name => _desc.DescriptionString();
    public bool IsHardware => !IsSoftware;
    public bool IsSoftware => (_desc.Flags & DXGI_ADAPTER_FLAG.DXGI_ADAPTER_FLAG_SOFTWARE) != 0;
    public string DebugString => $"{Name} Hardware = {IsHardware} DeviceId = {_desc.DeviceId} VendorId = {_desc.VendorId}";

    public void Dispose() => _adapter.Dispose();

    public static implicit operator IDXGIAdapter3*(in DXGIAdapter adapter) => adapter._adapter;
    public static implicit operator IUnknown*(in DXGIAdapter adapter) => (IUnknown*)adapter._adapter.Get();
}
