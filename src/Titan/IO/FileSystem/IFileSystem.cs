using Titan.Core.IO;

namespace Titan.IO.FileSystem;

/// <summary>
/// The public API to interact with files
/// </summary>
public interface IFileSystem : IService
{
    FileHandle Open(ReadOnlySpan<char> path, FilePathType type, bool createIfNotExist = false);
    void Close(ref FileHandle handle);
    int Read(in FileHandle handle, Span<byte> buffer, ulong offset = 0UL);
    int Write(in FileHandle handle, ReadOnlySpan<byte> content, ulong offset = 0UL);
    long GetLength(in FileHandle handle);
    void Truncate(in FileHandle handle);
}
