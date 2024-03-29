namespace Titan.Platform.Win32;

public unsafe struct WNDCLASSEXA
{
    public uint CbSize;
    public uint Style;
    public delegate* unmanaged<HWND, WindowMessage, nuint, nuint, nint> LpFnWndProc;
    public int CbClsExtra;
    public int CbWndExtra;
    public HINSTANCE HInstance;
    public nint HIcon;
    public HCURSOR HCursor;
    public nint HbrBackground;
    public byte* LpszMenuName;
    public byte* LpszClassName;
    public nint HIconSm;
}
