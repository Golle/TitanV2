using Titan.Input;

namespace Titan.Windows.Win32.Events;

internal record struct Win32KeyUpEvent(KeyCode Code) : IWin32Event
{
    public static int Id => EventTypes.KeyUp;
}
