using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32.DBT;

namespace Titan.Platform.Win32;

public static unsafe partial class User32
{
    public const int GWLP_USERDATA = -21;

    private const string DllName = "User32";

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial ushort RegisterClassExW(
        WNDCLASSEXW* wndClassEx
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial ushort RegisterClassExA(
        WNDCLASSEXA* wndClassEx
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterClassW(
        char* LpszClassName,
        HINSTANCE hInstance
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HWND CreateWindowExW(
        WindowStylesEx dwExStyle,
        char* lpClassName,
        char* lpWindowName,
        WindowStyles dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        HWND hWndParent,
        nint hMenu,
        HINSTANCE hInstance,
        void* lpParam
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyWindow(HWND hWnd);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseWindow(HWND hWnd);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowTextW(
        HWND hWnd,
        char* lpString
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowPos(
        HWND hWnd,
        HWND hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        SetWindowPos uFlags
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AdjustWindowRect(
        RECT* lpRect,
        WindowStyles dwStyle,
        [MarshalAs(UnmanagedType.Bool)] bool bMenu
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint SetWindowLongPtrW(
        HWND hwnd,
        int nIndex,
        nint dwNewLong
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint GetWindowLongPtrW(
        HWND hwnd,
        int nIndex
    );


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint GetWindowLongPtrA(
        HWND hwnd,
        int nIndex
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint DefWindowProcW(
        HWND hWnd,
        WindowMessage msg,
        nuint wParam,
        nuint lParam
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PeekMessageW(
        MSG* lpMsg,
        HWND hWnd,
        uint wMsgFilterMin,
        uint wMsgFilterMax,
        uint removeMessage
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int GetMessageW(
        MSG* lpMsg,
        HWND hWnd,
        uint wMsgFilterMin,
        uint wMsgFilterMax
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TranslateMessage(
        MSG* lpMsg
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint DispatchMessageW(
        MSG* lpMsg
    );
    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial void PostQuitMessage(
        int nExitCode
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int ShowWindow(
        HWND hWnd,
        ShowWindowCommands nCmdShow
    );



    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(
        POINT* lpPoint
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial short GetAsyncKeyState(int button);


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ScreenToClient(
        HWND hWnd,
        POINT* lpPoint
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetCursorPos(
        int x,
        int y
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HDEVNOTIFY RegisterDeviceNotificationA(
        HANDLE hRecipient,
        void* NotificationFilter,
        DEVICE_NOTIFY_FLAGS Flags
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HDEVNOTIFY RegisterDeviceNotificationW(
        HANDLE hRecipient,
        void* NotificationFilter,
        DEVICE_NOTIFY_FLAGS Flags
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterDeviceNotification(
        HDEVNOTIFY Handle
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UpdateWindow(HWND hwnd);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RedrawWindow(HWND hWnd, RECT* lprcUpdate, void* /*HRGN*/ hrgnUpdate, uint* flags);


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HHOOK* SetWindowsHookExA(WindowsHookCodes idHook, delegate* unmanaged<HookCodes, nuint, nuint, nint> lpfn, HINSTANCE hmod, uint dwThreadId);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HHOOK* SetWindowsHookExW(WindowsHookCodes idHook, delegate* unmanaged<HookCodes, nuint, nuint, nint> lpfn, nint hmod, uint dwThreadId);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial nint CallNextHookEx(HHOOK* hhk, HookCodes nCode, nuint wParam, nuint lParam);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int GetSystemMetrics(SystemMetricCodes nIndex);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int MessageBoxW(
        HWND hWnd,
        char* lpText,
        char* lpCaption,
        uint uType
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int ShowCursor(int bShow);

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorInfo(
        CURSORINFO* pci
    );


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostThreadMessageW(
        uint idThread,
        WindowMessage Msg,
        nuint wParam,
        nuint lParam
    );


    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool PostMessageW(
        HWND hWnd,
        WindowMessage Msg,
        nuint wParam,
        nuint lParam
    );

    [LibraryImport(DllName, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ClipCursor(RECT* rect);
}
