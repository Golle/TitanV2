using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Input;
using Titan.Platform.Win32;
using Titan.Systems;
using Titan.Windows.Win32.Events;
using static Titan.Platform.Win32.User32;

namespace Titan.Windows.Win32;

internal unsafe partial struct Win32WindowSystem
{
    private const string ClassName = nameof(Win32WindowSystem);

    [System(SystemStage.Init)]
    public static void CreateWindow(Window* window, Win32MessageQueue* queue, IConfigurationManager configurationManager)
    {
        //NOTE(Jens): Make sure the queue is zeroed out. 
        *queue = default;

        var config = configurationManager.GetConfigOrDefault<WindowConfig>();

        window->SetTitle(config.Title ?? "Win32 Window");
        window->Height = (int)config.Height;
        window->Width = (int)config.Width;
        window->Windowed = config.Windowed;
        window->X = config.X;
        window->Y = config.Y;

        Logger.Trace<Win32Window>($"Creating Win32 Window. Width = {window->Width} Height = {window->Height} Windowed = {config.Windowed} Title = {config.Title}");
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
                Logger.Error<Win32WindowSystem>($"Failed to register class. LastWin32ErrorCode = {lastError}");
            }
        }
        HWND parent = default;
        WindowStylesEx windowStyleEx = 0;

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
            Right = window->Width + windowOffset,
            Bottom = window->Height + windowOffset
        };
        if (!AdjustWindowRect(&windowRect, windowStyle, false))
        {
            Logger.Warning<Win32WindowSystem>($"Failed to {nameof(AdjustWindowRect)}.");
        }

        var x = config.X;
        var y = config.Y;
        if (x < 0 || y < 0)
        {
            //NOTE(Jens): This will use the primary monitor. 
            var screenWidth = GetSystemMetrics(SystemMetricCodes.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetricCodes.SM_CYSCREEN);

            x = (screenWidth - window->Width) / 2;
            y = (screenHeight - window->Height) / 2;

        }

        HWND handle;
        fixed (char* pClassName = ClassName)
        {
            handle = CreateWindowExW(
                windowStyleEx,
                pClassName,
                window->Title,
                windowStyle,
                x,
                y,
                windowRect.Right - windowRect.Left,
                windowRect.Bottom - windowRect.Top,
                parent,
                hMenu: 0,
                instance,
                queue
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

        ShowWindow(handle, ShowWindowCommands.SW_SHOW);
    }


    [System(SystemStage.Shutdown)]
    public static void DestroyWindow(Window* window)
    {
        if (window->Handle == 0)
        {
            Logger.Trace<Win32WindowSystem>("No Window Handle set, can't destroy window.");
            return;
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

        *window = default;
    }

    [System(SystemStage.Last)]
    public static void MessagePump(in Window window, ref Win32MessageQueue queue)
    {
        // Pump the messages and put them on the queue (This is something we might want to do async, maybe start a thread in CreateWindow)

        if (!queue.HasEvents())
        {
            return;
        }

        var count = queue.EventCount;
        for (var i = 0; i < count; ++i)
        {
            if (!queue.TryReadEvent(out var @event))
            {
                break;
            }

            switch (@event.Id)
            {
                case EventTypes.KeyDown:
                    ref readonly var keyDownEvent = ref @event.As<Win32KeyDownEvent>();
                    Logger.Info($"Key Down: {keyDownEvent.Code} (Repeat = {keyDownEvent.Repeat})");
                    break;
                case EventTypes.KeyUp:
                    ref readonly var keyUpEvent = ref @event.As<Win32KeyUpEvent>();
                    Logger.Info($"Key Down: {keyUpEvent.Code}");
                    break;
                default:
                    Logger.Warning<Win32WindowSystem>($"Win32 Message not handled. Id = {@event.Id}");
                    break;
            }
        }
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
            Logger.Trace<Win32Window>($"No message queue, message discarded. Message = {message} wparam = 0x{wParam:X8} lParam = 0x{lParam:X8}");
            return DefWindowProcW(hwnd, message, wParam, lParam);
        }
        var queue = (Win32MessageQueue*)userData;
        //NOTE(Jens): Do we want another layer here? that processes the windows messages

        switch (message)
        {
            case WindowMessage.WM_KEYDOWN:
                queue->Push(new Win32KeyDownEvent(KeyCode.A, false));
                break;
            case WindowMessage.WM_CLOSE:
                //NOTE(Jens): This should be an event, and quit message should be handled by the engine and not the Win32 API.
                PostQuitMessage(0);
                break;
            default:
                break;
        }

        return DefWindowProcW(hwnd, message, wParam, lParam);
    }
}
