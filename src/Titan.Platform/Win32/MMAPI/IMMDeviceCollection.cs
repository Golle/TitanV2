using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32.MMAPI;

public unsafe struct IMMDeviceCollection : INativeGuid
{
    public static Guid* Guid => IID.IID_IMMDeviceCollection;

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
    public HRESULT GetCount(uint* pcDevices)
        => ((delegate* unmanaged[Stdcall]<void*, uint*, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), pcDevices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT Item(uint ndevice, IMMDevice** ppDevice)
        => ((delegate* unmanaged[Stdcall]<void*, uint, IMMDevice**, HRESULT>)_vtbl[4])(Unsafe.AsPointer(ref this), ndevice, ppDevice);
}
