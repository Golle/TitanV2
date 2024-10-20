using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

public static unsafe partial class Ole32
{
    private const string DllName = "Ole32";
    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT CoCreateInstance(
        Guid* rclsid,
        void* pUnkOuter,
        CLSCTX dwClsContext,
        Guid* riid,
        void** ppv
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void CoTaskMemFree(void* ptr);


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HRESULT PropVariantClear(
        PROPVARIANT* pvar
    );
}
