using System.Diagnostics;
using Titan.Core.IO;
using Titan.Core.Logging;

namespace Titan.FileSystem;

public record struct FileSystemArgs(string AppDataName, string EnginePath, string ContentPath);

internal class FileSystem<TFileApi> : IFileSystem where TFileApi : INativeFileApi
{
    private readonly FileApi<TFileApi>[] _fileApis = new FileApi<TFileApi>[(int)FilePathType.Count];
    
    public bool Init(FileSystemArgs args)
    {
        _fileApis[(int)FilePathType.AppData] = new FileApi<TFileApi>(PathResolver.GetAppDataPath(args.AppDataName), false);
        _fileApis[(int)FilePathType.Temp] = new FileApi<TFileApi>(PathResolver.GetTempPath(args.AppDataName), false);
        _fileApis[(int)FilePathType.Logs] = new FileApi<TFileApi>(PathResolver.GetLogsPath(args.AppDataName), false);
        _fileApis[(int)FilePathType.Content] = new FileApi<TFileApi>(args.ContentPath, true);
        _fileApis[(int)FilePathType.Engine] = new FileApi<TFileApi>(args.EnginePath, true);

        for (var i = 0; i < _fileApis.Length; ++i)
        {
            ref readonly var api = ref _fileApis[i];
            if (!TryCreateDirectory(api.BasePath))
            {
                Logger.Error<FileSystem<TFileApi>>($"Failed to create the directory. Type = {(FilePathType)i} Path = {api.BasePath}");
                return false;
            }
        }
        return true;
    }

    private static bool TryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            Logger.Error<FileApi<TFileApi>>($"Failed to create the directory with {e.GetType()}. Message = {e.Message} Path = {path}");
            return false;
        }
        return true;
    }

    public bool Shutdown()
    {
        Array.Clear(_fileApis);
        return true;
    }

    public FileHandle Open(ReadOnlySpan<char> path, FilePathType type)
    {
        Debug.Assert(type != FilePathType.Count);
        ref readonly var fileApi = ref _fileApis[(int)type];
        var handle = fileApi.Open(path);
        return new(handle, type, fileApi.IsReadOnly);
    }

    public void Close(ref FileHandle handle)
    {
        _fileApis[(int)handle.Type].Close(ref handle.NativeFileHandle);
        handle = default;
    }

    public int Read(in FileHandle handle, Span<byte> buffer, ulong offset)
        => _fileApis[(int)handle.Type].Read(handle.NativeFileHandle, buffer, offset);

    public long GetLength(in FileHandle handle) 
        => _fileApis[(int)handle.Type].GetLength(handle.NativeFileHandle);
}
