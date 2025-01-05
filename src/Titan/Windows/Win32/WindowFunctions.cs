using Titan.Core.Maths;
using Titan.Input;

namespace Titan.Windows.Win32;

internal readonly unsafe struct WindowFunctions(
    delegate*<nuint, Point> getRelativeCursorPosition,
    delegate*<Point> getAbsoluteCursorPosition,
    delegate*<nuint, Point, void> setCursorPosition,
    delegate*<MouseButton, bool> isButtonDown,
    delegate*<nuint, char*, bool> setTitle,
    delegate*<nuint, void> close,
    delegate*<nuint, ref bool, void> toggleTopMost,
    delegate*<nuint, bool, void> showCursor,
    delegate*<nuint, bool, void> clipCursor,
    delegate*<nuint, uint, uint, void> resize,
    delegate*<nuint, bool> isInFocus
)
{
    public readonly delegate*<nuint, Point> GetRelativeCursorPosition = getRelativeCursorPosition;
    public readonly delegate*<Point> GetAbsoluteCursorPosition = getAbsoluteCursorPosition;
    public readonly delegate*<nuint, Point, void> SetCursorPosition = setCursorPosition;
    public readonly delegate*<MouseButton, bool> IsButtonDown = isButtonDown;
    public readonly delegate*<nuint, char*, bool> SetTitle = setTitle;
    public readonly delegate*<nuint, void> Close = close;
    public readonly delegate*<nuint, ref bool, void> ToggleTopMost = toggleTopMost;
    public readonly delegate*<nuint, bool, void> ShowCursor = showCursor;
    public readonly delegate*<nuint, bool, void> ClipCursor = clipCursor;
    public readonly delegate*<nuint, uint, uint, void> Resize = resize;
    public readonly delegate*<nuint, bool> IsInFocus = isInFocus;
}
