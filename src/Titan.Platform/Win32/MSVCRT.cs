using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

public static unsafe partial class MSVCRT
{
    private const string DllName = "msvcrt.dll";
    [LibraryImport(DllName)]
    public static partial int wcslen(char* str);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int memcmp(void* ptr1, void* ptr2, ulong count);

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int wcsncmp(char* str1, char* str2, nuint count);
    
}
