using System.Runtime.InteropServices;
using Titan.Core.Strings;

namespace Titan.Assets;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct FileDescriptor
{
    public uint Offset;
    public uint Length;

    public bool IsEmpty() => Length == 0;

    //#if !RELEASE
    public StringRef AssetPath;
    public StringRef BinaryAssetPath;
    //#endif
}
