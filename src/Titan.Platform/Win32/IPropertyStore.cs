using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32;
public unsafe struct IPropertyStore : INativeGuid
{
    public static Guid* Guid => IID.IID_IPropertyStore;

    private void** _vtbl;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
        => ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, HRESULT>)_vtbl[0])(Unsafe.AsPointer(ref this), riid, ppvObject);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddRef()
        => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[1])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Release()
        => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[2])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetCount(uint* propertyCount)
        => ((delegate* unmanaged[Stdcall]<void*, uint*, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), propertyCount);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetAt(uint index, PROPERTYKEY* pkey)
        => ((delegate* unmanaged[Stdcall]<void*, uint, PROPERTYKEY*, HRESULT>)_vtbl[4])(Unsafe.AsPointer(ref this), index, pkey);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetValue(PROPERTYKEY* pkey, PROPVARIANT* pv)
        => ((delegate* unmanaged[Stdcall]<void*, PROPERTYKEY*, PROPVARIANT*, HRESULT>)_vtbl[5])(Unsafe.AsPointer(ref this), pkey, pv);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT SetValue(PROPERTYKEY* pkey, PROPVARIANT* pv)
        => ((delegate* unmanaged[Stdcall]<void*, PROPERTYKEY*, PROPVARIANT*, HRESULT>)_vtbl[6])(Unsafe.AsPointer(ref this), pkey, pv);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT Commit()
        => ((delegate* unmanaged[Stdcall]<void*, HRESULT>)_vtbl[7])(Unsafe.AsPointer(ref this));
}
