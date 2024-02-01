using Titan.Application;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.IO.FileSystem;

namespace Titan.IO;
internal class FileSystemModule<TFileApi> : IModule where TFileApi : INativeFileApi
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        //NOTE(Jens): If nothing is set it will use a relative directory from the executable.
        //NOTE(Jens): Engine path is only used during development to support hot reloading of engine assets. In release mode all assets will be in the same folder
        var contentPath = config.ContentPath ?? Path.Combine(GlobalConfiguration.BasePath, "Content");
#if DEBUG
        var enginePath = config.EnginePath ?? contentPath;
#else
        var enginePath = contentPath;
#endif

        var fileSystem = new FileSystem<TFileApi>(config.Name, enginePath, contentPath);
        if (!fileSystem.Init())
        {
            Logger.Error<FileSystemModule<TFileApi>>($"Failed to init the {nameof(FileSystem<TFileApi>)}.");
            return false;
        }

        builder.AddService<IFileSystem, FileSystem<TFileApi>>(fileSystem);
        return true;
    }

    public static bool Init(IApp app) 
        => true;

    public static bool Shutdown(IApp app)
    {
        app.GetService<FileSystem<TFileApi>>()
            .Shutdown();
        return true;
    }
}
