namespace Titan.Windows.Win32.Events;

internal record struct Win32QuitEvent(int ExitCode) : IWin32Event
{
    public static int Id => EventTypes.Quit;
}
