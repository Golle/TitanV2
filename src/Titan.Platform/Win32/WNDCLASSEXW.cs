namespace Titan.Platform.Win32;

public unsafe struct WNDCLASSEXW
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
    public char* LpszMenuName;
    public char* LpszClassName;
    public nint HIconSm;
}
