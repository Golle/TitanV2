using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
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
    public static void CreateWindow(Window* window, Win32MessageQueue* queue, IThreadManager threadManager, IConfigurationManager configurationManager)
    {
        //NOTE(Jens): Make sure the queue is zeroed out. 
        *queue = default;

        var config = configurationManager.GetConfigOrDefault<WindowConfig>();
        window->Functions = Win32Functions.GetFunctionPointers();
        window->SetTitle(config.Title ?? "Win32 Window");
        window->Height = (int)config.Height;
        window->Width = (int)config.Width;
        window->Windowed = config.Windowed;
        window->X = config.X;
        window->Y = config.Y;
        window->Queue = queue;

        window->WindowThread = threadManager.Create(&CreateAndStartWindow, window, true);

        while (window->Handle == 0)
        {
            Thread.Sleep(1);
        }
    }


    [System(SystemStage.Shutdown)]
    public static void DestroyWindow(Window* window, IThreadManager threadManager)
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

        threadManager.Join(window->WindowThread);
        threadManager.Destroy(ref window->WindowThread);

        *window = default;
    }

    [UnmanagedCallersOnly]
    private static int CreateAndStartWindow(void* context)
    {
        var window = (Window*)context;
        var queue = (Win32MessageQueue*)window->Queue;

        Logger.Trace<Win32WindowSystem>($"Creating Win32 Window. Width = {window->Width} Height = {window->Height} Windowed = {window->Windowed} Title = {new string(window->Title, 0, window->TitleLength)}");
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
        //if (!config.Resizable)
        //{
        //    windowStyle ^= WindowStyles.WS_THICKFRAME;
        //}
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

        if (window->X < 0 || window->Y < 0)
        {
            //NOTE(Jens): This will use the primary monitor. 
            var screenWidth = GetSystemMetrics(SystemMetricCodes.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetricCodes.SM_CYSCREEN);

            window->X = (screenWidth - window->Width) / 2;
            window->Y = (screenHeight - window->Height) / 2;
        }

        HWND handle;
        fixed (char* pClassName = ClassName)
        {
            handle = CreateWindowExW(
                windowStyleEx,
                pClassName,
                window->Title,
                windowStyle,
                window->X,
                window->Y,
                windowRect.Right - windowRect.Left,
                windowRect.Bottom - windowRect.Top,
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
        ShowWindow(handle, ShowWindowCommands.SW_SHOW);

        while (window->Active)
        {
            MSG msg;
            //NOTE(Jens): For some reason the GetMessageW returns -1 when the window is closed.
            var result = GetMessageW(&msg, handle, 0, 0);
            if (result is 0 or -1)
            {
                queue->Push(new Win32QuitEvent(0));
                window->Active = false;
                break;
            }
            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }

        return 1;
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
                case EventTypes.Close:
                    Logger.Info<Win32WindowSystem>("Close message received");
                    PostQuitMessage(0);
                    break;
                case EventTypes.Quit:
                    Logger.Info<Win32WindowSystem>("Quit message received!");
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
            Logger.Trace<Win32WindowSystem>($"No message queue, message discarded. Message = {message} wparam = 0x{wParam:X8} lParam = 0x{lParam:X8}");
            return DefWindowProcW(hwnd, message, wParam, lParam);
        }
        var window = (Window*)userData;
        var queue = (Win32MessageQueue*)window->Queue;

        //NOTE(Jens): Do we want another layer here? that processes the windows messages

        switch (message)
        {
            case WindowMessage.WM_KEYDOWN:
                queue->Push(new Win32KeyDownEvent(KeyCode.A, false));
                break;
            case WindowMessage.WM_KEYUP:
                queue->Push(new Win32KeyUpEvent(KeyCode.A));
                break;
            case WindowMessage.WM_CLOSE:
                queue->Push(new Win32CloseEvent());
                break;
        }
        return DefWindowProcW(hwnd, message, wParam, lParam);
    }
}
