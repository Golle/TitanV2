using Titan.Core.Maths;
using Titan.Platform.Win32;

namespace Titan.Windows.Win32;
internal static unsafe class Win32Functions
{
    private static Point GetRelativeCursorPosition(nuint handle)
    {
        POINT point;
        if (User32.GetCursorPos(&point) && User32.ScreenToClient(handle, &point))
        {
            return new Point(point.X, point.Y);
        }

        //Logger.Error("Failed to get the Cursor position.", typeof(Win32Functions));
        return default;
    }

    private static bool SetTitle(nuint handle, char* title)
        => User32.SetWindowTextW(handle, title);

    private static void Close(nuint handle)
        => User32.CloseWindow(handle);

    public static WindowFunctions GetFunctionPointers() => new(
        &GetRelativeCursorPosition,
        &SetTitle,
        &Close
    );


}
