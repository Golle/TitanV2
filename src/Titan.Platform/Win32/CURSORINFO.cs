namespace Titan.Platform.Win32;

public struct CURSORINFO
{
    public uint cbSize;
    public CURSOR_STATE flags;
    public HCURSOR hCursor;
    public POINT ptScreenPos;
}
