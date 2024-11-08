namespace Titan.Platform.Win32.DBT;

public struct HDEVNOTIFY
{
    public nuint Value;

    public bool IsValid => Value != 0;
    public bool IsInvalid => Value == 0;
}
