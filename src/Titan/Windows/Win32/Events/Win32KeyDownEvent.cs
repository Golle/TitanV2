using Titan.Input;

namespace Titan.Windows.Win32.Events;

internal readonly struct Win32KeyDownEvent(KeyCode code, bool repeat) : IWin32Event
{
    public static int Id => EventTypes.KeyDown;
    public readonly KeyCode Code = code;
    public readonly bool Repeat = repeat;
}

internal readonly struct Win32KeyUpEvent(KeyCode code) : IWin32Event
{
    public static int Id => EventTypes.KeyDown;
    public readonly KeyCode Code = code;
}

internal readonly struct Win32QuitEvent(int code) : IWin32Event
{
    public static int Id => EventTypes.Quit;
    public readonly int ExitCode = code;
}

internal readonly struct Win32CloseEvent : IWin32Event
{
    public static int Id => EventTypes.Close;
}
