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
        => User32.CloseWindow(handle);

    private static bool IsButtonDown(MouseButton button)
        => (User32.GetAsyncKeyState((int)button) & 0x8000) != 0;

    private static void ToggleTopMost(nuint handle, ref bool isTopMost)
    {
        isTopMost = !isTopMost;
        User32.SetWindowPos(
            handle,
            isTopMost ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST,
            0, 0, 0, 0,
            SetWindowPos.SWP_NOMOVE | SetWindowPos.SWP_NOSIZE | SetWindowPos.SWP_NOACTIVATE
        );
    }

    private static void PostShowCursor(nuint handle, bool showCursor)
        => User32.PostMessageW(handle, Win32WindowSystem.WM_TOGGLE_CURSOR, (nuint)(showCursor ? 1 : 0), 0);

    public static WindowFunctions GetFunctionPointers() => new(
        &GetRelativeCursorPosition,
        &GetAbsoluteCursorPosition,
        &SetCursorPosition,
        &IsButtonDown,
        &SetTitle,
        &Close,
        &ToggleTopMost,
        &PostShowCursor
    );
}
