using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.D3D12;

using unsafe D3D12MessageFunc = delegate* unmanaged<D3D12_MESSAGE_CATEGORY, D3D12_MESSAGE_SEVERITY, D3D12_MESSAGE_ID, byte*, void*, void>;

[Guid("2852dd88-b484-4c0c-b6b1-67168500e600")]
public unsafe struct ID3D12InfoQueue1 : INativeGuid
{
    public static Guid* Guid => IID.IID_ID3D12InfoQueue1;
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

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, SetMessageCountLimit)
    //    HRESULT(STDMETHODCALLTYPE* SetMessageCountLimit)(
    //        ID3D12InfoQueue1* This,
    //        _In_ UINT64 MessageCountLimit);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, ClearStoredMessages)
    //    void (STDMETHODCALLTYPE* ClearStoredMessages ) (
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetMessage)
    //    HRESULT(STDMETHODCALLTYPE* GetMessage)(
    //        ID3D12InfoQueue1* This,
    //        _In_ UINT64 MessageIndex,
    //        _Out_writes_bytes_opt_(*pMessageByteLength)  D3D12_MESSAGE* pMessage,
    //        _Inout_  SIZE_T* pMessageByteLength);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumMessagesAllowedByStorageFilter)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesAllowedByStorageFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumMessagesDeniedByStorageFilter)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesDeniedByStorageFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumStoredMessages)
    //    UINT64(STDMETHODCALLTYPE* GetNumStoredMessages)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumStoredMessagesAllowedByRetrievalFilter)
    //    UINT64(STDMETHODCALLTYPE* GetNumStoredMessagesAllowedByRetrievalFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumMessagesDiscardedByMessageCountLimit)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesDiscardedByMessageCountLimit)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetMessageCountLimit)
    //    UINT64(STDMETHODCALLTYPE* GetMessageCountLimit)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddStorageFilterEntries)
    //    HRESULT(STDMETHODCALLTYPE* AddStorageFilterEntries)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* GetStorageFilter)(
    //        ID3D12InfoQueue1* This,
    //        _Out_writes_bytes_opt_(* pFilterByteLength) D3D12_INFO_QUEUE_FILTER *pFilter,
    //        _Inout_ SIZE_T *pFilterByteLength);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, ClearStorageFilter)
    //    void (STDMETHODCALLTYPE* ClearStorageFilter ) (
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushEmptyStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushEmptyStorageFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushCopyOfStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushCopyOfStorageFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushStorageFilter)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PopStorageFilter)
    //    void (STDMETHODCALLTYPE* PopStorageFilter ) (
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetStorageFilterStackSize)
    //    UINT(STDMETHODCALLTYPE* GetStorageFilterStackSize)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddRetrievalFilterEntries)
    //    HRESULT(STDMETHODCALLTYPE* AddRetrievalFilterEntries)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* GetRetrievalFilter)(
    //        ID3D12InfoQueue1* This,
    //        _Out_writes_bytes_opt_(* pFilterByteLength) D3D12_INFO_QUEUE_FILTER *pFilter,
    //        _Inout_ SIZE_T *pFilterByteLength);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, ClearRetrievalFilter)
    //    void (STDMETHODCALLTYPE* ClearRetrievalFilter ) (
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushEmptyRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushEmptyRetrievalFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushCopyOfRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushCopyOfRetrievalFilter)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushRetrievalFilter)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PopRetrievalFilter)
    //    void (STDMETHODCALLTYPE* PopRetrievalFilter ) (
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetRetrievalFilterStackSize)
    //    UINT(STDMETHODCALLTYPE* GetRetrievalFilterStackSize)(
    //        ID3D12InfoQueue1* This);

    //    DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddMessage)
    //    HRESULT(STDMETHODCALLTYPE* AddMessage)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_CATEGORY Category,
    //        _In_  D3D12_MESSAGE_SEVERITY Severity,
    //        _In_  D3D12_MESSAGE_ID ID,
    //        _In_  LPCSTR pDescription);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddApplicationMessage)
    //    HRESULT(STDMETHODCALLTYPE* AddApplicationMessage)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_SEVERITY Severity,
    //        _In_  LPCSTR pDescription);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, SetBreakOnCategory)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnCategory)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_CATEGORY Category,
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, SetBreakOnSeverity)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnSeverity)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_SEVERITY Severity,
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, SetBreakOnID)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnID)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_ID ID,
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetBreakOnCategory)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnCategory)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_CATEGORY Category);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetBreakOnSeverity)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnSeverity)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_SEVERITY Severity);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetBreakOnID)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnID)(
    //        ID3D12InfoQueue1* This,
    //        _In_ D3D12_MESSAGE_ID ID);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, SetMuteDebugOutput)
    //    void (STDMETHODCALLTYPE* SetMuteDebugOutput ) (
    //        ID3D12InfoQueue1* This,
    //        _In_ BOOL bMute);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetMuteDebugOutput)
    //    BOOL(STDMETHODCALLTYPE* GetMuteDebugOutput)(
    //        ID3D12InfoQueue1* This);

    public HRESULT RegisterMessageCallback(D3D12MessageFunc CallbackFunc, D3D12_MESSAGE_CALLBACK_FLAGS flags, void* pContext, uint* pCallbackCookie)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12MessageFunc, D3D12_MESSAGE_CALLBACK_FLAGS, void*, uint*, HRESULT>)_vtbl[38])(Unsafe.AsPointer(ref this), CallbackFunc, flags, pContext, pCallbackCookie);

    public HRESULT UnregisterMessageCallback(uint CallbackCookie)
        => ((delegate* unmanaged[Stdcall]<void*, uint, HRESULT>)_vtbl[39])(Unsafe.AsPointer(ref this), CallbackCookie);
}
