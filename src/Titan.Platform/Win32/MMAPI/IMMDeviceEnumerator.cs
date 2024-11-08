using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32.MMAPI;

public unsafe struct IMMDeviceEnumerator : INativeGuid
{
    public static Guid* Guid => IID.IID_IMMDeviceEnumerator;

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
    public HRESULT EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, IMMDeviceCollection** ppDevices)
        => ((delegate* unmanaged[Stdcall]<void*, EDataFlow, DeviceState, IMMDeviceCollection**, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), dataFlow, stateMask, ppDevices);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, IMMDevice** ppDevice)
        => ((delegate* unmanaged[Stdcall]<void*, EDataFlow, ERole, IMMDevice**, HRESULT>)_vtbl[4])(Unsafe.AsPointer(ref this), dataFlow, role, ppDevice);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetDevice(char* pwstrId, IMMDevice** ppDevice)
        => ((delegate* unmanaged[Stdcall]<void*, char*, IMMDevice**, HRESULT>)_vtbl[5])(Unsafe.AsPointer(ref this), pwstrId, ppDevice);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT RegisterEndpointNotificationCallback(void* pClient)
        => ((delegate* unmanaged[Stdcall]<void*, void*, HRESULT>)_vtbl[6])(Unsafe.AsPointer(ref this), pClient);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT UnregisterEndpointNotificationCallback(void* pClient)
        => ((delegate* unmanaged[Stdcall]<void*, void*, HRESULT>)_vtbl[7])(Unsafe.AsPointer(ref this), pClient);
}
