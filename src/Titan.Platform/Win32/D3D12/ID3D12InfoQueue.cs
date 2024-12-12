using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.D3D12;

[Guid("0742A90B-C387-483F-B946-30A7E4E61458")]
public unsafe struct ID3D12InfoQueue : INativeGuid
{
    public static Guid* Guid => IID.IID_ID3D12InfoQueue;
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
    public HRESULT SetMessageCountLimit(ulong MessageCountLimit)
        => ((delegate* unmanaged[Stdcall]<void*, ulong, HRESULT>)_vtbl[3])(Unsafe.AsPointer(ref this), MessageCountLimit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearStoredMessages()
        => ((delegate* unmanaged[Stdcall]<void*, void>)_vtbl[4])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetMessage(uint MessageIndex, D3D12_MESSAGE* pMessage, nuint* pMessageByteLength)
        => ((delegate* unmanaged[Stdcall]<void*, uint, D3D12_MESSAGE*, nuint*, HRESULT>)_vtbl[5])(Unsafe.AsPointer(ref this), MessageIndex, pMessage, pMessageByteLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetNumMessagesAllowedByStorageFilter()
        => ((delegate* unmanaged[Stdcall]<void*, ulong>)_vtbl[6])(Unsafe.AsPointer(ref this));

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumMessagesDeniedByStorageFilter)
    //UINT64(STDMETHODCALLTYPE* GetNumMessagesDeniedByStorageFilter)(
    //    ID3D12InfoQueue* This);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetNumMessagesStored()
        => ((delegate* unmanaged[Stdcall]<void*, ulong>)_vtbl[8])(Unsafe.AsPointer(ref this));

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetNumStoredMessagesAllowedByRetrievalFilter)
    //UINT64(STDMETHODCALLTYPE* GetNumStoredMessagesAllowedByRetrievalFilter)(
    //    ID3D12InfoQueue* This);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetNumMessagesDiscardedByMessageCountLimit()
        => ((delegate* unmanaged[Stdcall]<void*, ulong>)_vtbl[10])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetMessageCountLimit()
        => ((delegate* unmanaged[Stdcall]<void*, ulong>)_vtbl[11])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT AddStorageFilterEntries(D3D12_INFO_QUEUE_FILTER* pFilter)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_INFO_QUEUE_FILTER*, HRESULT>)_vtbl[12])(Unsafe.AsPointer(ref this), pFilter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetStorageFilter(D3D12_INFO_QUEUE_FILTER* pFilter, nuint* pFilterByteLength)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_INFO_QUEUE_FILTER*, nuint*, HRESULT>)_vtbl[13])(Unsafe.AsPointer(ref this), pFilter, pFilterByteLength);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearStorageFilter()
        => ((delegate* unmanaged[Stdcall]<void*, void>)_vtbl[14])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT PushEmptyStorageFilter()
        => ((delegate* unmanaged[Stdcall]<void*, HRESULT>)_vtbl[15])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT PushCopyOfStorageFilter()
        => ((delegate* unmanaged[Stdcall]<void*, HRESULT>)_vtbl[16])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT PushStorageFilter(D3D12_INFO_QUEUE_FILTER* pFilter)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_INFO_QUEUE_FILTER*, HRESULT>)_vtbl[17])(Unsafe.AsPointer(ref this), pFilter);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopStorageFilter()
        => ((delegate* unmanaged[Stdcall]<void*, void>)_vtbl[18])(Unsafe.AsPointer(ref this));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetStorageFilterStackSize()
        => ((delegate* unmanaged[Stdcall]<void*, uint>)_vtbl[19])(Unsafe.AsPointer(ref this));

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddRetrievalFilterEntries)
    //HRESULT(STDMETHODCALLTYPE* AddRetrievalFilterEntries)(
    //ID3D12InfoQueue* This,
    //_In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetRetrievalFilter)
    //HRESULT(STDMETHODCALLTYPE* GetRetrievalFilter)(
    //ID3D12InfoQueue* This,
    //_Out_writes_bytes_opt_(* pFilterByteLength) D3D12_INFO_QUEUE_FILTER *pFilter,
    //    _Inout_ SIZE_T *pFilterByteLength);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, ClearRetrievalFilter)
    //void (STDMETHODCALLTYPE* ClearRetrievalFilter ) (
    //    ID3D12InfoQueue* This);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushEmptyRetrievalFilter)
    //HRESULT(STDMETHODCALLTYPE* PushEmptyRetrievalFilter)(
    //    ID3D12InfoQueue* This);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushCopyOfRetrievalFilter)
    //HRESULT(STDMETHODCALLTYPE* PushCopyOfRetrievalFilter)(
    //    ID3D12InfoQueue* This);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PushRetrievalFilter)
    //HRESULT(STDMETHODCALLTYPE* PushRetrievalFilter)(
    //ID3D12InfoQueue* This,
    //_In_ D3D12_INFO_QUEUE_FILTER * pFilter);

    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, PopRetrievalFilter)
    //void (STDMETHODCALLTYPE* PopRetrievalFilter ) (
    //    ID3D12InfoQueue* This);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, GetRetrievalFilterStackSize)
    //UINT(STDMETHODCALLTYPE* GetRetrievalFilterStackSize)(
    //    ID3D12InfoQueue* This);
        
    //DECLSPEC_XFGVIRT(ID3D12InfoQueue, AddMessage)
    //HRESULT(STDMETHODCALLTYPE* AddMessage)(
    //ID3D12InfoQueue* This,
    //_In_ D3D12_MESSAGE_CATEGORY Category,
    //_In_  D3D12_MESSAGE_SEVERITY Severity,
    //_In_  D3D12_MESSAGE_ID ID,
    //_In_  LPCSTR pDescription);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT AddApplicationMessage(D3D12_MESSAGE_SEVERITY Severity, char* pDescription)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_SEVERITY, char*, HRESULT>)_vtbl[29])(Unsafe.AsPointer(ref this), Severity, pDescription);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT SetBreakOnCategory(D3D12_MESSAGE_CATEGORY Category, bool bEnable)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_CATEGORY, bool, HRESULT>)_vtbl[30])(Unsafe.AsPointer(ref this), Category, bEnable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT SetBreakOnSeverity(D3D12_MESSAGE_SEVERITY Severity, int bEnable)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_SEVERITY, int, HRESULT>)_vtbl[31])(Unsafe.AsPointer(ref this), Severity, bEnable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT SetBreakOnID(D3D12_MESSAGE_ID ID, bool bEnable)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_ID, bool, HRESULT>)_vtbl[32])(Unsafe.AsPointer(ref this), ID, bEnable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBreakOnCategory(D3D12_MESSAGE_CATEGORY Category)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_CATEGORY, bool>)_vtbl[33])(Unsafe.AsPointer(ref this), Category);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBreakOnSeverity(D3D12_MESSAGE_SEVERITY Severity)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_SEVERITY, bool>)_vtbl[34])(Unsafe.AsPointer(ref this), Severity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBreakOnID(D3D12_MESSAGE_ID ID)
        => ((delegate* unmanaged[Stdcall]<void*, D3D12_MESSAGE_ID, bool>)_vtbl[35])(Unsafe.AsPointer(ref this), ID);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMuteDebugOutput(bool bMute)
        => ((delegate* unmanaged[Stdcall]<void*, bool, void>)_vtbl[36])(Unsafe.AsPointer(ref this), bMute);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetMuteDebugOutput()
        => ((delegate* unmanaged[Stdcall]<void*, bool>)_vtbl[37])(Unsafe.AsPointer(ref this));
}
