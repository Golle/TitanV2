using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32.DXGI;

[Guid("119E7452-DE9E-40fe-8806-88F90C12B441")]
public unsafe struct IDXGIDebug : INativeGuid
{
    public static Guid* Guid => IID.IID_IDXGIDebug;
    private void** _vtbl;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
        => ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, HRESULT>)_vtbl[0])(Unsafe.AsPointer(ref this), riid, ppvObject);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddRef() => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[1])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Release() => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[2])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT ReportLiveObjects(Guid* apiid, DXGI_DEBUG_RLO_FLAGS flags)
        => ((delegate* unmanaged[Stdcall]<void*, Guid*, DXGI_DEBUG_RLO_FLAGS, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), apiid, flags);
}
