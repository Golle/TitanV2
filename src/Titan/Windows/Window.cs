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

    public int TitleLength;
    public fixed char Title[MaxTitleSize];

    public void* Queue;
    public NativeThreadHandle WindowThread;
    public HDEVNOTIFY DeviceNotificationHandle;
    public bool Windowed;
    public bool Active;

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

    public readonly bool IsButtonDown(MouseButton button)
        => Functions.IsButtonDown(button);

    public void Close()
        => Functions.Close(Handle);

    public readonly void ToggleTopMost() 
        => Functions.ToggleTopMost(Handle, ref Unsafe.AsRef(in IsTopMost)); // we use Unsafe here to avoid taking a mutable reference to the Window.
}
