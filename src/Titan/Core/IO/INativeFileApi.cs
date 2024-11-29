namespace Titan.Core.IO;

public interface INativeFileApi
{
    static abstract NativeFileHandle Open(ReadOnlySpan<char> path, FileAccess access, bool createIfNotExist);
    static abstract int Read(in NativeFileHandle handle, Span<byte> buffer, ulong offset);
    static abstract unsafe int Read(in NativeFileHandle handle, void* buffer, nuint bufferSize, ulong offset);
    static abstract int Write(in NativeFileHandle handle, ReadOnlySpan<byte> buffer);
    static abstract void Close(ref NativeFileHandle handle);
    static abstract long GetLength(in NativeFileHandle handle);
    static abstract void Truncate(in NativeFileHandle handle);
    static abstract FileTime GetFileTime(in NativeFileHandle handle);
}
