using System.Diagnostics;
using Titan.Core.Logging;

namespace Titan.Core.IO;

internal readonly struct FileApi<TFileApi>(string basePath, bool readOnly) : IFileApi
    where TFileApi : INativeFileApi
{
    public readonly bool IsReadOnly = readOnly;
    public readonly string BasePath = basePath;
    public NativeFileHandle Open(ReadOnlySpan<char> path, bool createIfNotExists)
    {
        Debug.Assert(BasePath != null, $"{GetType().Name} has not been initialized.");
        var fullPath = Path.GetFullPath($"{BasePath}{Path.DirectorySeparatorChar}{path}");
        return TFileApi.Open(fullPath, IsReadOnly ? FileAccess.Read : FileAccess.ReadWrite, createIfNotExists);
    }

    public void Close(ref NativeFileHandle handle)
        => TFileApi.Close(ref handle);

    public int Read(in NativeFileHandle handle, Span<byte> buffer, ulong offset = 0)
        => TFileApi.Read(handle, buffer, offset);

    public int Write(in NativeFileHandle handle, ReadOnlySpan<byte> content, ulong offset = 0)
    {
        if (IsReadOnly)
        {
            Logger.Error<FileApi<TFileApi>>($"Trying to {nameof(Write)} on a handle that is read only");
            return -1;
        }
        return TFileApi.Write(handle, content);
    }

    public int Append(in NativeFileHandle handle, ReadOnlySpan<byte> content)
    {
        throw new NotImplementedException("Not sure how to implement this since the file is open already.");
    }

    public long GetLength(in NativeFileHandle handle)
        => TFileApi.GetLength(handle);

    public void Truncate(in NativeFileHandle handle)
    {
        if (IsReadOnly)
        {
            Logger.Error<FileApi<TFileApi>>($"Trying to {nameof(Truncate)} a handle that is read only");
            return;
        }
        TFileApi.Truncate(handle);
    }
}
