using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32.DXGI;


public static unsafe partial class DXGICommon
{
    private const string DllName = "dxgi";
    private const string DebugDllName = "Dxgidebug";

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT CreateDXGIFactory1(Guid* riid, void** ppFactory);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT CreateDXGIFactory2(
        DXGI_CREATE_FACTORY_FLAGS Flags,
        Guid* riid,
        void** ppFactory
    );

    [LibraryImport(DebugDllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT DXGIGetDebugInterface(
        Guid* riid,
        void** ppDebug
    );

    [LibraryImport(DebugDllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT DXGIGetDebugInterface1(
        uint Flags,
        Guid* riid,
        void** pDebug
    );
}
