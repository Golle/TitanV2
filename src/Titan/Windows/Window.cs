using System.Runtime.CompilerServices;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Input;
using Titan.Platform.Win32.DBT;
using Titan.Resources;
using Titan.UI.Widgets;
using Titan.Windows.Win32;

namespace Titan.Windows;

[UnmanagedResource]
public unsafe partial struct Window
{
    public const int MaxTitleSize = 128;

    internal WindowFunctions Functions;

    internal nuint Handle;
    internal void* Queue;
    internal NativeThreadHandle WindowThread;
    internal HDEVNOTIFY DeviceNotificationHandle;
    internal int TitleLength;
    internal fixed char Title[MaxTitleSize];


    public int Width, Height;
    public int WidthWithFrame, HeightWithFrame;
    public int X, Y;
    public int ScreenWidth, ScreenHeight;
    public bool Windowed;
    public bool Active;
    public bool CursorVisible;

    private bool IsTopMost;

    public Size Size => new(Width, Height);
    public SizeF SizeF => new(Width, Height);

    public void SetTitle(string title)
    {
        fixed (char* pTitle = Title)
        {
            // clear the title before writing the new one.
            MemoryUtils.Init(pTitle, MaxTitleSize);

            title.CopyTo(new(pTitle, MaxTitleSize));
            TitleLength = title.Length;

            Functions.SetTitle(Handle, pTitle);
        }
    }

    public readonly Point GetRelativeCursorPosition()
        => Functions.GetRelativeCursorPosition(Handle);

    public readonly Point GetAbsoluteCursorPosition()
        => Functions.GetAbsoluteCursorPosition();

    public readonly bool IsButtonDown(MouseButton button)
        => Functions.IsButtonDown(button);

    public readonly void SetCursorPosition(Point point)
        => Functions.SetCursorPosition(Handle, point);

    public readonly void Close()
        => Functions.Close(Handle);

    public readonly void ToggleTopMost()
        => Functions.ToggleTopMost(Handle, ref Unsafe.AsRef(in IsTopMost)); // we use Unsafe here to avoid taking a mutable reference to the Window.

    public readonly void ShowCursor(bool showCursor)
        => Functions.ShowCursor(Handle, showCursor);

    public readonly void KeepCursorInWindow(bool insideWindow)
    => Functions.ClipCursor(Handle, insideWindow);

    /// <summary>
    /// Resize the window.
    /// <remarks>This endpoint will move the window so it's centered</remarks>
    /// <remarks>The size provided should be compatible with the modes returned my the GPU</remarks>
    /// </summary>
    /// <param name="width">Window width without border</param>
    /// <param name="height">Window height without border</param>
    public readonly void Resize(uint width, uint height)
        => Functions.Resize(Handle, width, height);

    public readonly bool IsFocused() 
        => Functions.IsInFocus(Handle);
}
