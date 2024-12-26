namespace Titan.Windows.Win32.Events;

internal record struct Win32ResizeEvent(uint Width, uint Height) : IWin32Event
{
    public static int Id => EventTypes.Resize;
}
