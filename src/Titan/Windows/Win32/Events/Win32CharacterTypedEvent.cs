namespace Titan.Windows.Win32.Events;

internal record struct Win32CharacterTypedEvent(char Character) : IWin32Event
{
    public static int Id => EventTypes.CharacterTyped;
}
