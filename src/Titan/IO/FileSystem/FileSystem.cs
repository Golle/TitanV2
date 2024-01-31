using System.Diagnostics;
using Titan.Core.IO;
using Titan.Core.Logging;

namespace Titan.IO.FileSystem;

internal class FileSystem<TFileApi>(string appdataName, string enginePath, string contentPath) : IFileSystem where TFileApi : INativeFileApi
{
    private readonly FileApi<TFileApi>[] _fileApis = new FileApi<TFileApi>[(int)FilePathType.Count];

    public bool Init()
    {
        _fileApis[(int)FilePathType.AppData] = new FileApi<TFileApi>(PathResolver.GetAppDataPath(appdataName), false);
        _fileApis[(int)FilePathType.Temp] = new FileApi<TFileApi>(PathResolver.GetTempPath(appdataName), false);
        _fileApis[(int)FilePathType.Logs] = new FileApi<TFileApi>(PathResolver.GetLogsPath(appdataName), false);
        _fileApis[(int)FilePathType.Configs] = new FileApi<TFileApi>(PathResolver.GetConfigsPath(appdataName), false);
        _fileApis[(int)FilePathType.Content] = new FileApi<TFileApi>(contentPath, true);
        _fileApis[(int)FilePathType.Engine] = new FileApi<TFileApi>(enginePath, true);

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
            Logger.Trace<FileSystem<TFileApi>>($"Create directory. Path = {path}");
            Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            Logger.Error<FileApi<TFileApi>>($"Failed to create the directory with {e.GetType()}. Message = {e.Message} Path = {path}");
            return false;
        }
        return true;
    }

    public void Shutdown()
    {
        Array.Clear(_fileApis);
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
