using Titan.Core.Maths;
using Titan.Input;

namespace Titan.Windows.Win32;

internal readonly unsafe struct WindowFunctions(
    delegate*<nuint, Point> getRelativeCursorPosition,
    delegate*<MouseButton, bool> isButtonDown,
    delegate*<nuint, char*, bool> setTitle,
    delegate*<nuint, void> close,
    delegate*<nuint, ref bool, void> toggleTopMost
)
{
    public readonly delegate*<nuint, Point> GetRelativeCursorPosition = getRelativeCursorPosition;
    public readonly delegate*<MouseButton, bool> IsButtonDown = isButtonDown;
    public readonly delegate*<nuint, char*, bool> SetTitle = setTitle;
    public readonly delegate*<nuint, void> Close = close;
    public readonly delegate*<nuint, ref bool, void> ToggleTopMost = toggleTopMost;
}
