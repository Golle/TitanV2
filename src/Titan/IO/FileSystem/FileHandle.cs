using System.Diagnostics.CodeAnalysis;
using Titan.Core.IO;

namespace Titan.IO.FileSystem;

public struct FileHandle
{
    internal NativeFileHandle NativeFileHandle;
    public readonly FilePathType Type;
    public readonly bool IsReadOnly;

    [UnscopedRef]
    public ref readonly NativeFileHandle Handle => ref NativeFileHandle;
    public bool IsValid() => NativeFileHandle.IsValid();
    public bool IsInvalid() => NativeFileHandle.IsInvalid();
    internal FileHandle(NativeFileHandle handle, FilePathType type, bool isReadOnly)
    {
        NativeFileHandle = handle;
        Type = type;
        IsReadOnly = isReadOnly;
    }
}
