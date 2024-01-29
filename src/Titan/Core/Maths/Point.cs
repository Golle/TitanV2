using System.Runtime.InteropServices;

namespace Titan.Core.Maths;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Point(int x, int y)
{
    public int X = x;
    public int Y = y;

    //TODO(Jens): Add more functions etc.
}
