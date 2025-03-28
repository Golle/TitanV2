using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

public unsafe partial struct Kernel32
{
    private const string DllName = "kernel32";

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HMODULE GetModuleHandleW(
        char* lpModuleName
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void* VirtualAlloc(
        void* lpAddress,
        nuint dwSize,
        AllocationType flAllocationType,
        AllocationProtect flProtect
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool VirtualFree(
        void* lpAddress,
        nuint dwSize,
        AllocationType dwFreeType
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void GetSystemInfo(
        SYSTEM_INFO* lpSystemInfo
    );

    [LibraryImport(DllName, SetLastError = false)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !DEBUG
    [SuppressGCTransition] // This causes issues when a debugger is attached.
#endif
    public static partial uint WaitForSingleObject(
        HANDLE hHandle,
        uint dwMilliseconds
    );

    public static HANDLE CreateEventW(SecurityAttributes* lpEventAttributes, int bManualReset, int bInitialState, string name)
    {
        fixed (char* pName = name)
        {
            return CreateEventW(lpEventAttributes, bManualReset, bInitialState, pName);
        }
    }


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HANDLE CreateEventW(
        SecurityAttributes* lpEventAttributes,
        int bManualReset,
        int bInitialState,
        char* lpName
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HANDLE CreateEventA(
        SecurityAttributes* lpEventAttributes,
        int bManualReset,
        int bInitialState,
        byte* lpName
    );
    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetEvent(HANDLE hEvent);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseHandle(HANDLE handle);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HANDLE CreateFileW(
        char* lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        SecurityAttributes* lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        HANDLE hTemplateFile
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReadFile(
        HANDLE hFile,
        void* lpBuffer,
        uint nNumberOfBytesToRead,
        uint* lpNumberOfBytesRead,
        OVERLAPPED* lpOverlapped
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool WriteFile(
        HANDLE hFile,
        void* lpBuffer,
        uint nNumberOfBytesToWrite,
        uint* lpNumberOfBytesWritten,
        OVERLAPPED* lpOverlapped
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetFileSizeEx(
        HANDLE hFile,
        LARGE_INTEGER* lpFileSize
    );


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetFileTime(
        HANDLE hFile,
        FILETIME* lpCreationTime,
        FILETIME* lpLastAccessTime,
        FILETIME* lpLastWriteTime
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetEndOfFile(
        HANDLE hFile
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetFilePointerEx(
        HANDLE hFile,
        LARGE_INTEGER liDistanceToMove,
        LARGE_INTEGER* lpNewFilePointer,
        FileMoveMethod dwMoveMethod
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HANDLE CreateThread(
        SecurityAttributes* lpThreadAttributes,
        nuint dwStackSize,
        delegate* unmanaged<void*, int> lpStartAddress,
        void* lpParameter,
        uint dwCreationFlags,
        uint* lpThreadId
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void Sleep(uint milliseconds);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [SuppressGCTransition]
    public static partial uint GetCurrentThreadId();

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial uint ResumeThread(
        HANDLE hThread
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SwitchToThread();

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetDllDirectoryW(
        char* lpPathName
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AllocConsole();

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint GetConsoleWindow();

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AttachConsole(uint dWProcessId);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeConsole();

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HMODULE LoadLibraryA(byte* lpLibFileName);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HMODULE LoadLibraryW(char* lpLibFileName);
}
