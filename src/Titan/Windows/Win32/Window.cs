using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Platform.Win32;
using Titan.Resources;

namespace Titan.Windows.Win32;

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
}
