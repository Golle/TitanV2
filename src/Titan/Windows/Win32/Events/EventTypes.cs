namespace Titan.Windows.Win32.Events;
internal static class EventTypes
{
    public const int KeyDown = 10001;
    public const int KeyUp = 10002;
    public const int CharacterTyped = 10004;


    public const int AudioDeviceArrival = 30001;
    public const int AudioDeviceRemoveComplete = 30002;

    public const int Quit = 20001;
    public const int Close = 20002;
    public const int Resize = 20003;


    public const int LostFocus = 40001;
    public const int GainedFocus = 40002;

}
