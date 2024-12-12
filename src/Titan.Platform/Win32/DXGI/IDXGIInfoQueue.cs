using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32.D3D12;

namespace Titan.Platform.Win32.DXGI;

[Guid("D67441C7-672A-476f-9E82-CD55B44949CE")]
public unsafe struct IDXGIInfoQueue : INativeGuid
{
    public static Guid* Guid => IID.IID_IDXGIInfoQueue;
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

    //    DECLSPEC_XFGVIRT(IDXGIInfoQueue, SetMessageCountLimit)
    //    HRESULT(STDMETHODCALLTYPE* SetMessageCountLimit)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  UINT64 MessageCountLimit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearStoredMessages(Guid Producer)
        => ((delegate* unmanaged[Stdcall]<void*, Guid, void>)_vtbl[4])(Unsafe.AsPointer(ref this), Producer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetMessage(
        Guid Producer,
        ulong MessageIndex,
        DXGI_INFO_QUEUE_MESSAGE* pMessage,
        nuint* pMessageByteLength)
        => ((delegate* unmanaged[Stdcall]<void*, Guid, ulong, DXGI_INFO_QUEUE_MESSAGE*, nuint*, HRESULT>)_vtbl[5])(Unsafe.AsPointer(ref this), Producer, MessageIndex, pMessage, pMessageByteLength);


    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetNumStoredMessagesAllowedByRetrievalFilters)
    //    UINT64(STDMETHODCALLTYPE* GetNumStoredMessagesAllowedByRetrievalFilters)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetNumStoredMessages(Guid Producer)
        => ((delegate* unmanaged[Stdcall]<void*, Guid, ulong>)_vtbl[7])(Unsafe.AsPointer(ref this), Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetNumMessagesDiscardedByMessageCountLimit)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesDiscardedByMessageCountLimit)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetMessageCountLimit)
    //    UINT64(STDMETHODCALLTYPE* GetMessageCountLimit)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetNumMessagesAllowedByStorageFilter)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesAllowedByStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetNumMessagesDeniedByStorageFilter)
    //    UINT64(STDMETHODCALLTYPE* GetNumMessagesDeniedByStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, AddStorageFilterEntries)
    //    HRESULT(STDMETHODCALLTYPE* AddStorageFilterEntries)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_FILTER* pFilter);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* GetStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _Out_writes_bytes_opt_(*pFilterByteLength)  DXGI_INFO_QUEUE_FILTER* pFilter,
    //        /* [annotation] */
    //        _Inout_  SIZE_T* pFilterByteLength);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, ClearStorageFilter)
    //    void (STDMETHODCALLTYPE* ClearStorageFilter ) (
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushEmptyStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushEmptyStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushDenyAllStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushDenyAllStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushCopyOfStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushCopyOfStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushStorageFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushStorageFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_FILTER* pFilter);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PopStorageFilter)
    //    void (STDMETHODCALLTYPE* PopStorageFilter ) (
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetStorageFilterStackSize)
    //    UINT(STDMETHODCALLTYPE* GetStorageFilterStackSize)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, AddRetrievalFilterEntries)
    //    HRESULT(STDMETHODCALLTYPE* AddRetrievalFilterEntries)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_FILTER* pFilter);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* GetRetrievalFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _Out_writes_bytes_opt_(*pFilterByteLength)  DXGI_INFO_QUEUE_FILTER* pFilter,
    //        /* [annotation] */
    //        _Inout_  SIZE_T* pFilterByteLength);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, ClearRetrievalFilter)
    //    void (STDMETHODCALLTYPE* ClearRetrievalFilter ) (
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushEmptyRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushEmptyRetrievalFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushDenyAllRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushDenyAllRetrievalFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushCopyOfRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushCopyOfRetrievalFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PushRetrievalFilter)
    //    HRESULT(STDMETHODCALLTYPE* PushRetrievalFilter)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_FILTER* pFilter);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, PopRetrievalFilter)
    //    void (STDMETHODCALLTYPE* PopRetrievalFilter ) (
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetRetrievalFilterStackSize)
    //    UINT(STDMETHODCALLTYPE* GetRetrievalFilterStackSize)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, AddMessage)
    //    HRESULT(STDMETHODCALLTYPE* AddMessage)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_CATEGORY Category,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_SEVERITY Severity,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_ID ID,
    //        /* [annotation] */
    //        _In_  LPCSTR pDescription);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, AddApplicationMessage)
    //    HRESULT(STDMETHODCALLTYPE* AddApplicationMessage)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_INFO_QUEUE_MESSAGE_SEVERITY Severity,
    //        /* [annotation] */
    //        _In_  LPCSTR pDescription);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, SetBreakOnCategory)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnCategory)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_CATEGORY Category,
    //        /* [annotation] */
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, SetBreakOnSeverity)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnSeverity)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_SEVERITY Severity,
    //        /* [annotation] */
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, SetBreakOnID)
    //    HRESULT(STDMETHODCALLTYPE* SetBreakOnID)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_ID ID,
    //        /* [annotation] */
    //        _In_  BOOL bEnable);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetBreakOnCategory)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnCategory)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_CATEGORY Category);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetBreakOnSeverity)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnSeverity)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_SEVERITY Severity);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetBreakOnID)
    //    BOOL(STDMETHODCALLTYPE* GetBreakOnID)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  DXGI_INFO_QUEUE_MESSAGE_ID ID);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, SetMuteDebugOutput)
    //    void (STDMETHODCALLTYPE* SetMuteDebugOutput ) (
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer,
    //        /* [annotation] */
    //        _In_  BOOL bMute);

    //DECLSPEC_XFGVIRT(IDXGIInfoQueue, GetMuteDebugOutput)
    //    BOOL(STDMETHODCALLTYPE* GetMuteDebugOutput)(
    //        IDXGIInfoQueue* This,
    //        /* [annotation] */
    //        _In_ DXGI_DEBUG_ID Producer);
}
