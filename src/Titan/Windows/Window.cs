using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Threading;
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
    public bool Windowed;
    public bool Active;

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

    public void Close()
        => Functions.Close(Handle);
}
