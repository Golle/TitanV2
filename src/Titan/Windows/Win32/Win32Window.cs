using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Platform.Win32;
using static Titan.Platform.Win32.User32;

namespace Titan.Windows.Win32;
internal unsafe class Win32Window(string title) : IWindow
{
    // We only support a single window so this is fine.
    private const string ClassName = nameof(Win32Window);

    private HWND _windowHandle;
    private int _width;
    private int _height;
    public string? Title { get; private set; }
    public nint NativeHandle => _windowHandle;
    public uint Height => (uint)_height;
    public uint Width => (uint)_width;

    public bool Init(WindowConfig config, ManagedResource<Win32MessagePump> messagePump)
    {
        if (config.Title != null)
        {
            title = config.Title;
        }

        var height = (int)config.Height;
        var width = (int)config.Width;

        Logger.Trace<Win32Window>($"Creating Win32 Window. Width = {width} Height = {height} Windowed = {config.Windowed} Title = {title}");

        HINSTANCE instance = Kernel32.GetModuleHandleW(null);

        fixed (char* pClassName = ClassName)
        {
            WNDCLASSEXW windowClass = new()
            {
                CbSize = (uint)sizeof(WNDCLASSEXW),
                HCursor = default,
                HIcon = default,
                HIconSm = default,
                HbrBackground = default,
                LpFnWndProc = &WindowProc,
                HInstance = instance,
                LpszClassName = pClassName,
                Style = 0,
                LpszMenuName = null,
                CbClsExtra = 0,
                CbWndExtra = 0
            };
            var classResult = RegisterClassExW(&windowClass);
            if (classResult == 0)
            {
                var lastError = Marshal.GetLastWin32Error();
                Logger.Error<Win32Window>($"Failed to register class. LastWin32ErrorCode = {lastError}");
            }
        }

        HWND parent = default;
        WindowStylesEx windowStyleEx = 0;
        //NOTE(Jens): Implement this when needed, good for debugging.
        //if (config.AlwaysOnTop)
        //{
        //    windowStyleEx |= WINDOWSTYLES_EX.WS_EX_TOPMOST;
        //}

        var windowStyle = WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_VISIBLE;
        if (!config.Resizable)
        {
            windowStyle ^= WindowStyles.WS_THICKFRAME;
        }

        const int windowOffset = 100;
        var windowRect = new RECT
        {
            Left = windowOffset,
            Top = windowOffset,
            Right = width + windowOffset,
            Bottom = height + windowOffset
        };
        if (!AdjustWindowRect(&windowRect, windowStyle, false))
        {
            Logger.Warning<Win32Window>($"Failed to {nameof(AdjustWindowRect)}.");
        }


        var x = config.X;
        var y = config.Y;
        if (x < 0 || y < 0)
        {
            //NOTE(Jens): This will use the primary monitor. 
            var screenWidth = GetSystemMetrics(SystemMetricCodes.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetricCodes.SM_CYSCREEN);

            x = (screenWidth - width) / 2;
            y = (screenHeight - height) / 2;

        }



        fixed (char* pTitle = title)
        fixed (char* pClassName = ClassName)
        {
            _windowHandle = CreateWindowExW(
                windowStyleEx,
                pClassName,
                pTitle,
                windowStyle,
                x,
                y,
                windowRect.Right - windowRect.Left,
                windowRect.Bottom - windowRect.Top,
                parent,
                hMenu: 0,
                instance,
                (void*)messagePump.Handle
            );

            if (!_windowHandle.IsValid)
            {
                var lastError = Marshal.GetLastWin32Error();
                Logger.Error<Win32Window>($"Failed to create the window. Win32ErrorCode = {lastError}");
                UnregisterClassW(pClassName, instance);
                return false;
            }
        }

        _width = width;
        _height = height;
        Title = title;

        ShowWindow(_windowHandle, ShowWindowCommands.SW_SHOW);

        return true;
    }

    public void Shutdown()
    {
        DestroyWindow(_windowHandle);
        fixed (char* pClassName = ClassName)
        {
            HINSTANCE instance = Kernel32.GetModuleHandleW(null);
            UnregisterClassW(pClassName, instance);
        }
    }


    public bool Update()
    {
        MSG msg;
        while (PeekMessageW(&msg, 0, 0, 0, 1))
        {
            if (msg.Message == WindowMessage.WM_QUIT)
            {
                return false;
            }

            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }

        return true;
    }

    public bool UpdateBlocking()
    {
        MSG msg;
        //NOTE(Jens): For some reason the GetMessageW returns -1 when the window is closed.
        var result = GetMessageW(&msg, _windowHandle, 0, 0);
        if (result is 0 or -1)
        {
            return false;
        }

        TranslateMessage(&msg);
        DispatchMessageW(&msg);
        return true;
    }

    public bool SetTitle(ReadOnlySpan<char> newTitle)
    {
        //NOTE(Jens): This allocates memory, not sure we want this.
        Title = newTitle.ToString();
        Debug.Assert(_windowHandle.IsValid);
        fixed (char* pTitle = newTitle)
        {
            return SetWindowTextW(_windowHandle, pTitle);
        }
    }

    public Point GetRelativeCursorPosition()
    {
        POINT point;
        if (GetCursorPos(&point) && ScreenToClient(_windowHandle, &point))
        {
            return new Point(point.X, point.Y);
        }

        Logger.Error<Win32Window>("Failed to get the Cursor position.");
        return default;
    }

    [UnmanagedCallersOnly]
    private static nint WindowProc(HWND hwnd, WindowMessage message, nuint wParam, nuint lParam)
    {
        if (message == WindowMessage.WM_CREATE)
        {
            var create = (CREATESTRUCTW*)lParam;
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, (nint)create->lpCreateParams);
        }

        var userData = GetWindowLongPtrW(hwnd, GWLP_USERDATA);
        if (userData == 0)
        {
            Logger.Trace<Win32Window>($"No message pump, message discarded. Message = {message} wparam = 0x{wParam:X8} lParam = 0x{lParam:X8}");
            return DefWindowProcW(hwnd, message, wParam, lParam);
        }
        var handle = (ManagedResource<Win32MessagePump>)userData;
        Debug.Assert(handle.IsValid);

        var result = handle.Value.OnMessage(hwnd, message, wParam, lParam);
        if (result)
        {
            return 0;
        }

        return DefWindowProcW(hwnd, message, wParam, lParam);
    }
}
