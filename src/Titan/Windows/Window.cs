using System.Runtime.CompilerServices;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Input;
using Titan.Platform.Win32.DBT;
using Titan.Resources;
using Titan.Windows.Win32;

namespace Titan.Windows;

[UnmanagedResource]
internal unsafe partial struct Window
{
    public const int MaxTitleSize = 128;

    public WindowFunctions Functions;

    public nuint Handle;
    public int Width, Height;
    public int X, Y;

    public int ScreenWidth, ScreenHeight;

    public int TitleLength;
    public fixed char Title[MaxTitleSize];

    public void* Queue;
    public NativeThreadHandle WindowThread;
    public HDEVNOTIFY DeviceNotificationHandle;
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
}
