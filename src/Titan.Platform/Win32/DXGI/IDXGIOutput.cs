using System.Runtime.CompilerServices;

namespace Titan.Platform.Win32.DXGI;

public unsafe struct IDXGIOutput
{
    private void** _vtbl;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT QueryInterface(Guid* riid, void** ppvObject) => ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, HRESULT>)_vtbl[0])(Unsafe.AsPointer(ref this), riid, ppvObject);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddRef() => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[1])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Release() => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[2])(Unsafe.AsPointer(ref this));

    //DECLSPEC_XFGVIRT(IDXGIObject, SetPrivateData)
    //    HRESULT(STDMETHODCALLTYPE* SetPrivateData)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ REFGUID Name,
    //        /* [in] */ UINT DataSize,
    //        /* [annotation][in] */ 
    //        _In_reads_bytes_(DataSize)  const void* pData);

    //DECLSPEC_XFGVIRT(IDXGIObject, SetPrivateDataInterface)
    //    HRESULT(STDMETHODCALLTYPE* SetPrivateDataInterface)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ REFGUID Name,
    //        /* [annotation][in] */
    //        _In_opt_  const IUnknown* pUnknown);

    //DECLSPEC_XFGVIRT(IDXGIObject, GetPrivateData)
    //    HRESULT(STDMETHODCALLTYPE* GetPrivateData)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ REFGUID Name,
    //        /* [annotation][out][in] */
    //        _Inout_  UINT* pDataSize,
    //        /* [annotation][out] */
    //        _Out_writes_bytes_(*pDataSize)  void* pData);

    //DECLSPEC_XFGVIRT(IDXGIObject, GetParent)
    //    HRESULT(STDMETHODCALLTYPE* GetParent)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ REFIID riid,
    //        /* [annotation][retval][out] */
    //        _COM_Outptr_  void** ppParent);

    //DECLSPEC_XFGVIRT(IDXGIOutput, GetDesc)
    //    HRESULT(STDMETHODCALLTYPE* GetDesc)(
    //        IDXGIOutput* This,
    //        /* [annotation][out] */
    //        _Out_ DXGI_OUTPUT_DESC * pDesc);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetDisplayModeList(DXGI_FORMAT EnumFormat, uint Flags, uint* pNumModes, DXGI_MODE_DESC* pDesc)
     => ((delegate* unmanaged[Stdcall]<void*, DXGI_FORMAT, uint, uint*, DXGI_MODE_DESC*, HRESULT>)_vtbl[8])(Unsafe.AsPointer(ref this), EnumFormat, Flags, pNumModes, pDesc);

    //DECLSPEC_XFGVIRT(IDXGIOutput, FindClosestMatchingMode)
    //    HRESULT(STDMETHODCALLTYPE* FindClosestMatchingMode)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_  const DXGI_MODE_DESC* pModeToMatch,
    //        /* [annotation][out] */
    //        _Out_  DXGI_MODE_DESC* pClosestMatch,
    //        /* [annotation][in] */
    //        _In_opt_  IUnknown* pConcernedDevice);

    //DECLSPEC_XFGVIRT(IDXGIOutput, WaitForVBlank)
    //    HRESULT(STDMETHODCALLTYPE* WaitForVBlank)(
    //        IDXGIOutput* This);

    //    DECLSPEC_XFGVIRT(IDXGIOutput, TakeOwnership)
    //    HRESULT(STDMETHODCALLTYPE* TakeOwnership)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ IUnknown * pDevice,
    //        BOOL Exclusive);

    //    DECLSPEC_XFGVIRT(IDXGIOutput, ReleaseOwnership)
    //    void (STDMETHODCALLTYPE* ReleaseOwnership ) (
    //        IDXGIOutput* This);

    //    DECLSPEC_XFGVIRT(IDXGIOutput, GetGammaControlCapabilities)
    //    HRESULT(STDMETHODCALLTYPE* GetGammaControlCapabilities)(
    //        IDXGIOutput* This,
    //        /* [annotation][out] */
    //        _Out_ DXGI_GAMMA_CONTROL_CAPABILITIES * pGammaCaps);

    //DECLSPEC_XFGVIRT(IDXGIOutput, SetGammaControl)
    //    HRESULT(STDMETHODCALLTYPE* SetGammaControl)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_  const DXGI_GAMMA_CONTROL* pArray);

    //DECLSPEC_XFGVIRT(IDXGIOutput, GetGammaControl)
    //    HRESULT(STDMETHODCALLTYPE* GetGammaControl)(
    //        IDXGIOutput* This,
    //        /* [annotation][out] */
    //        _Out_ DXGI_GAMMA_CONTROL * pArray);

    //DECLSPEC_XFGVIRT(IDXGIOutput, SetDisplaySurface)
    //    HRESULT(STDMETHODCALLTYPE* SetDisplaySurface)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ IDXGISurface * pScanoutSurface);

    //DECLSPEC_XFGVIRT(IDXGIOutput, GetDisplaySurfaceData)
    //    HRESULT(STDMETHODCALLTYPE* GetDisplaySurfaceData)(
    //        IDXGIOutput* This,
    //        /* [annotation][in] */
    //        _In_ IDXGISurface * pDestination);

    //DECLSPEC_XFGVIRT(IDXGIOutput, GetFrameStatistics)
    //    HRESULT(STDMETHODCALLTYPE* GetFrameStatistics)(
    //        IDXGIOutput* This,
    //        /* [annotation][out] */
    //        _Out_ DXGI_FRAME_STATISTICS * pStats);
}
