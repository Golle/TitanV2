using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Threading;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DBT;
using Titan.Systems;
using Titan.Windows.Win32.Events;
using static Titan.Platform.Win32.User32;
using static Titan.Platform.Win32.WindowMessage;

namespace Titan.Windows.Win32;

internal unsafe partial struct Win32WindowSystem
{

    private const WindowStyles BorderlessFullscreen = WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE;

    //NOTE(Jens): We remove ths posibility to resize by pulling the edges. Doesn't really make sense for a game anyway. Cool feature, but not practical at this stage.
    private const WindowStyles Windowed = (WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_VISIBLE) & ~WindowStyles.WS_MAXIMIZEBOX & ~WindowStyles.WS_THICKFRAME;

    // keep track of the cursor so we can change it.
    public const WindowMessage WM_TOGGLE_CURSOR = WM_USER + 1;
    public const WindowMessage WM_CLIP_CURSOR_TO_SCREEN = WM_TOGGLE_CURSOR + 1;
    public const WindowMessage WM_WINDOW_RESIZE = WM_CLIP_CURSOR_TO_SCREEN + 1;

    public static readonly Size ScreenSize = new
    (
        GetSystemMetrics(SystemMetricCodes.SM_CXSCREEN),
        GetSystemMetrics(SystemMetricCodes.SM_CYSCREEN)
    );

    private const string ClassName = nameof(Win32WindowSystem);
    [System(SystemStage.Init)]
    public static void CreateWindow(Window* window, Win32MessageQueue* queue, IThreadManager threadManager, IConfigurationManager configurationManager)
    {
        //NOTE(Jens): Make sure the queue is zeroed out. 
        *queue = default;

        var config = configurationManager.GetConfigOrDefault<WindowConfig>();
        if (config.Resizable)
        {
            Logger.Warning<Win32WindowSystem>("Resizable is set to true, this is currently not supported.");
        }


        window->Functions = Win32Functions.GetFunctionPointers();
        window->SetTitle(config.Title ?? "Win32 Window");
        window->Height = (int)config.Height;
        window->Width = (int)config.Width;
        window->Windowed = config.Windowed;
        window->X = config.X;
        window->Y = config.Y;
        window->Queue = queue;
        window->ScreenHeight = ScreenSize.Height;
        window->ScreenWidth = ScreenSize.Width;
        window->WindowThread = threadManager.Create(&CreateAndStartWindow, window, true);
        //TODO(Jens): This should be handled in some nicer way. CreateEvent and SetEvent causes the debugger to deadlock though, so not sure what we can do.
        //NOTE(Jens): Update, this is not true anymore, it was due to GC supress
        while (window->Handle == 0)
        {
            Thread.Sleep(1);
        }

        CURSORINFO cursorInfo;
        cursorInfo.cbSize = (uint)sizeof(CURSORINFO);
        if (GetCursorInfo(&cursorInfo))
        {
            window->CursorVisible = cursorInfo.flags != CURSOR_STATE.CURSOR_HIDDEN;
        }

        window->KeepCursorInWindow(config.KeepCursorInside);
    }

    [System(SystemStage.Shutdown)]
    public static void DestroyWindow(Window* window, IThreadManager threadManager)
    {
        if (window->Handle == 0)
        {
            Logger.Trace<Win32WindowSystem>("No Window Handle set, can't destroy window.");
            return;
        }

        if (window->DeviceNotificationHandle.IsValid)
        {
            UnregisterDeviceNotification(window->DeviceNotificationHandle);
        }

        // Cast it into the fire!
        var handle = (HWND)window->Handle;
        if (handle.IsValid)
        {
            fixed (char* pClass = ClassName)
            {
                var instance = Kernel32.GetModuleHandleW(null);
                UnregisterClassW(pClass, instance);
            }

            User32.DestroyWindow(handle);
        }

        threadManager.Join(window->WindowThread);
        threadManager.Destroy(ref window->WindowThread);

        *window = default;
    }

#if !RELEASE
    [System]
#endif
    public static void Update(in Window window, in InputState inputState)
    {
        if (inputState.IsKeyReleased(KeyCode.F9))
        {
            Logger.Trace<Win32WindowSystem>("Toggle Top Most");
            window.ToggleTopMost();
        }

    }

    [UnmanagedCallersOnly]
    private static int CreateAndStartWindow(void* context)
    {
        var window = (Window*)context;

        Logger.Trace<Win32WindowSystem>($"Creating Win32 Window. Width = {window->Width} Height = {window->Height} Windowed = {window->Windowed} Title = {new string(window->Title, 0, window->TitleLength)}");
        HINSTANCE instance = Kernel32.GetModuleHandleW(null);

        fixed (char* pClassName = ClassName)
        {
            WNDCLASSEXW windowClass = new()
            {
                CbSize = (uint)sizeof(WNDCLASSEXW),
                HCursor = LoadCursorA(default, StandardCursorIDs.IDC_ARROW),
                HIcon = 0,
                HIconSm = 0,
                HbrBackground = 0,
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
                Logger.Error<Win32WindowSystem>($"Failed to register class. LastWin32ErrorCode = {lastError}");
            }
        }

        HWND parent = default;
        WindowStylesEx windowStyleEx = 0;//WindowStylesEx.WS_EX_TOPMOST;

        UpdateWindowSize(window);

        HWND handle;
        fixed (char* pClassName = ClassName)
        {
            handle = CreateWindowExW(
                windowStyleEx,
                pClassName,
                window->Title,
                window->Windowed ? Windowed : BorderlessFullscreen,
                window->X,
                window->Y,
                window->WidthWithFrame,
                window->HeightWithFrame,
                parent,
                hMenu: 0,
                instance,
                window
            );

            if (!handle.IsValid)
            {
                var lastError = Marshal.GetLastWin32Error();
                Logger.Error<Win32WindowSystem>($"Failed to create the window. Win32ErrorCode = {lastError}");
                UnregisterClassW(pClassName, instance);

                //TODO(Jens): implement fatal error message
            }
        }
        window->Handle = (nuint)handle.Value;
        window->Active = true;
        var queue = (Win32MessageQueue*)window->Queue;

        ShowWindow(handle, ShowWindowCommands.SW_SHOW);

        ref var active = ref window->Active;
        while (active)
        {
            MSG msg;
            //NOTE(Jens): For some reason the GetMessageW returns -1 when the window is closed.
            var result = GetMessageW(&msg, handle, 0, 0);
            if (result is 0 or -1)
            {
                active = false;
                break;
            }

            //NOTE(Jens): Some events are collected before the Window proc is called. This is to decrease the latency.
            switch (msg.Message)
            {
                case WM_TOGGLE_CURSOR:
                    //NOTE(Jens): ShowCursor has a counter inside, we don't want to increase it more than once.
                    if (window->CursorVisible && msg.WParam == 0)
                    {
                        ShowCursor(0);
                    }
                    else if (!window->CursorVisible && msg.WParam == 1)
                    {
                        ShowCursor(1);
                    }

                    window->CursorVisible = msg.WParam == 1;
                    continue;

                case WM_CLIP_CURSOR_TO_SCREEN:
                    var windowRect = new RECT
                    {
                        Left = window->X,
                        Top = window->Y,
                        Right = window->Width + window->X,
                        Bottom = window->Height + window->Y
                    };
                    ClipCursor(msg.WParam == 1 ? &windowRect : null);
                    continue;

                case WM_WINDOW_RESIZE:
                    window->Height = (int)msg.LParam;
                    window->Width = (int)msg.WParam;
                    window->Y = window->X = -1;
                    UpdateWindowSize(window);
                    var windowResult = SetWindowPos(handle, 0, window->X, window->Y, window->WidthWithFrame, window->HeightWithFrame, SetWindowPosFlags.SWP_ASYNCWINDOWPOS);
                    if (windowResult)
                    {
                        queue->Push(new Win32ResizeEvent((uint)window->Width, (uint)window->Height));
                    }
                    else
                    {
                        Logger.Error<Win32WindowSystem>("Failed to update the size and position of the Window.");
                    }

                    break;
                case WM_MOUSEWHEEL:
                    var delta = (short)(msg.WParam >> 16);
                    queue->Push(new Win32MouseWheelEvent(delta));
                    break;
            }

            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }

        Logger.Trace<Win32WindowSystem>("Window Message loop ended");
        return 1;
    }

    private static void UpdateWindowSize(Window* window)
    {
        if (window->Windowed)
        {
            const int windowOffset = 100;
            var rect = new RECT
            {
                Left = windowOffset,
                Top = windowOffset,
                Right = window->Width + windowOffset,
                Bottom = window->Height + windowOffset
            };

            // We'll have a border, so we have to calculate the screen size properly.
            var windowStyle = window->Windowed ? Windowed : BorderlessFullscreen;
            if (!AdjustWindowRect(&rect, windowStyle, false))
            {
                Logger.Warning<Win32WindowSystem>($"Failed to {nameof(AdjustWindowRect)}.");
            }

            if (window->X < 0 || window->Y < 0)
            {
                //NOTE(Jens): This will use the primary monitor. 
                window->X = (ScreenSize.Width - window->Width) / 2;
                window->Y = (ScreenSize.Height - window->Height) / 2;
            }

            window->WidthWithFrame = rect.Right - rect.Left;
            window->HeightWithFrame = rect.Bottom - rect.Top;
            return;
        }

        // Default full screen size and position.
        window->X = window->Y = 0;
        window->WidthWithFrame = window->Width = ScreenSize.Width;
        window->HeightWithFrame = window->Height = ScreenSize.Height;
    }

    [UnmanagedCallersOnly]
    private static nint WindowProc(HWND hwnd, WindowMessage message, nuint wParam, nuint lParam)
    {
        Window* window;
        if (message == WM_CREATE)
        {
            var create = (CREATESTRUCTW*)lParam;
            window = (Window*)create->lpCreateParams;
            SetWindowLongPtrW(hwnd, GWLP_USERDATA, (nint)window);

            DEV_BROADCAST_DEVICEINTERFACE_W filter = default;
            filter.dbcc_size = (uint)sizeof(DEV_BROADCAST_DEVICEINTERFACE_W);
            filter.dbcc_devicetype = (uint)DBT_DEVICE_TYPES.DBT_DEVTYP_DEVICEINTERFACE;
            filter.dbcc_classguid = DEVICEINTERFACE_AUDIO.DEVINTERFACE_AUDIO_RENDER;

            window->DeviceNotificationHandle = RegisterDeviceNotificationW(hwnd.Value, &filter, DEVICE_NOTIFY_FLAGS.DEVICE_NOTIFY_WINDOW_HANDLE);
            if (!window->DeviceNotificationHandle.IsValid)
            {
                Logger.Error<Win32WindowSystem>("Failed to register the window for Audio Device notifications.");
            }
        }

        var userData = GetWindowLongPtrW(hwnd, GWLP_USERDATA);
        if (userData == 0)
        {
            Logger.Trace<Win32WindowSystem>($"No message queue, message discarded. Message = {message} wparam = 0x{wParam:X8} lParam = 0x{lParam:X8}");
            return DefWindowProcW(hwnd, message, wParam, lParam);
        }
        window = (Window*)userData;
        var queue = (Win32MessageQueue*)window->Queue;

        //NOTE(Jens): Do we want another layer here? that processes the windows messages

        switch (message)
        {
            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
                {
                    var repeat = (lParam & 0x40000000) > 0;
                    var code = (int)wParam;
                    Debug.Assert(code is >= 0 and <= byte.MaxValue);
                    queue->Push(new Win32KeyDownEvent((KeyCode)code, repeat));
                    break;
                }
            case WM_KEYUP:
            case WM_SYSKEYUP:
                {
                    var code = (int)wParam;
                    Debug.Assert(code is >= 0 and <= byte.MaxValue);
                    queue->Push(new Win32KeyUpEvent((KeyCode)code));
                    break;
                }

            case WM_CHAR:
                var character = (char)wParam;
                queue->Push(new Win32CharacterTypedEvent(character));
                break;

            case WM_CLOSE:
                queue->Push(new Win32CloseEvent());
                break;

            case WM_KILLFOCUS:
                // If we loose focux and the cursor is hidden we show it again. 
                if (!window->CursorVisible)
                {
                    ShowCursor(1);
                    window->CursorVisible = true;
                }
                queue->Push(new Win32LostFocusEvent());
                break;

            case WM_SETFOCUS:
                queue->Push(new Win32GainedFocusEvent());
                break;

            //case WM_SIZE:
            //    var width = lParam & 0xffff;
            //    var height = (lParam >> 16) & 0xffff;
            //    queue->Push(new Win32ResizeEvent((uint)width, (uint)height));
            //    break;

            case WM_DEVICECHANGE:
                var devinterface = (DEV_BROADCAST_DEVICEINTERFACE_W*)lParam; // This is also the DEV_BROADCAST_HDR struct
                if (devinterface == null || devinterface->dbcc_devicetype != (nint)DBT_DEVICE_TYPES.DBT_DEVTYP_DEVICEINTERFACE || devinterface->dbcc_classguid != DEVICEINTERFACE_AUDIO.DEVINTERFACE_AUDIO_RENDER)
                {
                    // nothing we care about at the moment
                    break;
                }

                //NOTE(Jens): Add the device name for debugging?
                switch ((DBT_DEVICE_TYPES)wParam)
                {
                    case DBT_DEVICE_TYPES.DBT_DEVICEARRIVAL:
                        queue->Push(new AudioDeviceArrivalEvent());
                        break;
                    case DBT_DEVICE_TYPES.DBT_DEVICEREMOVECOMPLETE:
                        queue->Push(new AudioDeviceRemoveCompleteEvent());
                        break;
                }
                break;
        }

        return DefWindowProcW(hwnd, message, wParam, lParam);
    }
}
