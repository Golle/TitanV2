namespace Titan.Windows.Win32.Events;

internal record struct AudioDeviceArrivalEvent : IWin32Event
{
    public static int Id => EventTypes.AudioDeviceArrival;
}
