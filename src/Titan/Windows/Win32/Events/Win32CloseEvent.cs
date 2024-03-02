namespace Titan.Windows.Win32.Events;

internal record struct Win32CloseEvent : IWin32Event
{
    public static int Id => EventTypes.Close;
}
