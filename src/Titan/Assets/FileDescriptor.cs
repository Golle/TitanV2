using System.Runtime.InteropServices;
using Titan.Core.Strings;

namespace Titan.Assets;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FileDescriptor
{
    public uint Offset;
    public uint Length;

    //#if !RELEASE
    public StringRef AssetPath;
    //#endif
}
