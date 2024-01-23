namespace Titan.Core.IO;

internal interface IFileApi
{
    NativeFileHandle Open(ReadOnlySpan<char> path);
    void Close(ref NativeFileHandle handle);
    int Read(in NativeFileHandle handle, Span<byte> buffer, ulong offset = 0L);
    int Write(in NativeFileHandle handle, ReadOnlySpan<byte> content, ulong offset = 0L);
    int Append(in NativeFileHandle handle, ReadOnlySpan<byte> content);
    long GetLength(in NativeFileHandle handle);
}
