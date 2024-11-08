namespace Titan.Windows.Win32.Events;

internal record struct AudioDeviceRemoveCompleteEvent : IWin32Event
{
    public static int Id => EventTypes.AudioDeviceRemoveComplete;
}
