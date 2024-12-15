using System.Runtime.InteropServices;

namespace Titan.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    //NOTE(Jens): Make sure these are the same as the render API being used (only d3d12 at the moment)
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
