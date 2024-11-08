using System.Runtime.CompilerServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32.MMAPI;

public unsafe struct IMMDevice : INativeGuid
{
    public static Guid* Guid => IID.IID_IMMDevice;

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
    public HRESULT Activate(Guid* iid, CLSCTX dwClsCtx, void* pActivationParams, void** ppInterface)
        => ((delegate* unmanaged[Stdcall]<void*, Guid*, CLSCTX, void*, void**, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), iid, dwClsCtx, pActivationParams, ppInterface);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT OpenPropertyStore(StorageAccessMode stgmAccess, IPropertyStore** ppProperties)
        => ((delegate* unmanaged[Stdcall]<void*, StorageAccessMode, IPropertyStore**, HRESULT>)_vtbl[4])(Unsafe.AsPointer(ref this), stgmAccess, ppProperties);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetId(char** ppstrId)
        => ((delegate* unmanaged[Stdcall]<void*, char**, HRESULT>)_vtbl[5])(Unsafe.AsPointer(ref this), ppstrId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetState(uint* pdwState)
        => ((delegate* unmanaged[Stdcall]<void*, uint*, HRESULT>)_vtbl[6])(Unsafe.AsPointer(ref this), pdwState);
}
