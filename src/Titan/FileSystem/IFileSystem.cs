using Titan.Core.IO;

namespace Titan.FileSystem;

/// <summary>
/// The public API to interact with files
/// </summary>
public interface IFileSystem
{
    FileHandle Open(ReadOnlySpan<char> path, FilePathType type);
    void Close(ref FileHandle handle);
    int Read(in FileHandle handle, Span<byte> buffer, ulong offset = 0UL);
    long GetLength(in FileHandle handle);
    internal bool Init(FileSystemArgs args);
    internal bool Shutdown();
}
