using Titan.Input;

namespace Titan.Windows.Win32.Events;

internal record struct Win32KeyDownEvent(KeyCode Code, bool Repeat) : IWin32Event
{
    public static int Id => EventTypes.KeyDown;
}
