namespace Titan.Platform.Win32;

public unsafe struct CREATESTRUCTW
{
    public void* lpCreateParams;
    public nint hInstance;
    public nint hMenu;
    public HWND hwndParent;
    public int cy;
    public int cx;
    public int y;
    public int x;
    public int style;
    public char* lpszName;
    public char* lpszClass;
    public uint dwExStyle;
}
