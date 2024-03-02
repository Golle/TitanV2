using Titan.Core.Maths;

namespace Titan.Windows.Win32;

internal readonly unsafe struct WindowFunctions(
    delegate*<nuint, Point> getRelativeCursorPosition,
    delegate*<nuint, char*, bool> setTitle,
    delegate*<nuint, void> close
)
{
    public readonly delegate*<nuint, Point> GetRelativeCursorPosition = getRelativeCursorPosition;
    public readonly delegate*<nuint, char*, bool> SetTitle = setTitle;
    public readonly delegate*<nuint, void> Close = close;
}
