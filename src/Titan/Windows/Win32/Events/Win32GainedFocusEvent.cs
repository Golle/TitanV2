namespace Titan.Windows.Win32.Events;

internal record struct Win32GainedFocusEvent : IWin32Event
{
    public static int Id => EventTypes.GainedFocus;
}
