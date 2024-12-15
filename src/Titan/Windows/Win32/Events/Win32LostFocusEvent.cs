namespace Titan.Windows.Win32.Events;

internal record struct Win32LostFocusEvent : IWin32Event
{
    public static int Id => EventTypes.LostFocus;
}
