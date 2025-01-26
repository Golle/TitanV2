using System.Runtime.CompilerServices;
using Titan.Core.Maths;
using Titan.Input;
using Titan.Platform.Win32;

namespace Titan.Windows.Win32;
internal static unsafe class Win32Functions
{
    private static Point GetRelativeCursorPosition(nuint handle)
    {
        Unsafe.SkipInit(out POINT point);
        if (User32.GetCursorPos(&point) && User32.ScreenToClient(handle, &point))
        {
            return new Point(point.X, point.Y);
        }

        //Logger.Error("Failed to get the Cursor position.", typeof(Win32Functions));
        return default;
    }
    private static Point GetAbsoluteCursorPosition()
    {
        Unsafe.SkipInit(out POINT point);
        if (User32.GetCursorPos(&point))
        {
            return new Point(point.X, point.Y);
        }

        //Logger.Error("Failed to get the Cursor position.", typeof(Win32Functions));
        return default;
    }
    private static void SetCursorPosition(nuint handle, Point point)
        => User32.SetCursorPos(point.X, point.Y);

    private static bool SetTitle(nuint handle, char* title)
        => User32.SetWindowTextW(handle, title);

    private static void Close(nuint handle)
        => User32.PostMessageW(handle, WindowMessage.WM_CLOSE, 0, 0);

    private static bool IsButtonDown(MouseButton button)
        => (User32.GetAsyncKeyState((int)button) & 0x8000) != 0;

    private static void ToggleTopMost(nuint handle, ref bool isTopMost)
    {
        isTopMost = !isTopMost;
        User32.SetWindowPos(
            handle,
            isTopMost ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST,
            0, 0, 0, 0,
            SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE
        );
    }

    private static bool IsInFocus(nuint handle)
    {
        var focusHandle = (nuint)User32.GetForegroundWindow().Value;
        return handle == focusHandle;
    }

    private static void PostShowCursor(nuint handle, bool showCursor)
        => User32.PostMessageW(handle, Win32WindowSystem.WM_TOGGLE_CURSOR, (nuint)(showCursor ? 1 : 0), 0);

    private static void PostClipCursor(nuint handle, bool insideWindow)
        => User32.PostMessageW(handle, Win32WindowSystem.WM_CLIP_CURSOR_TO_SCREEN, (nuint)(insideWindow ? 1 : 0), 0);

    private static void Resize(nuint handle, uint width, uint height)
        => User32.PostMessageW(handle, Win32WindowSystem.WM_WINDOW_RESIZE, width, height);

    public static WindowFunctions GetFunctionPointers() => new(
        &GetRelativeCursorPosition,
        &GetAbsoluteCursorPosition,
        &SetCursorPosition,
        &IsButtonDown,
        &SetTitle,
        &Close,
        &ToggleTopMost,
        &PostShowCursor,
        &PostClipCursor,
        &Resize,
        &IsInFocus
    );
}
