using Titan.Core;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Events;
using Titan.IO.FileSystem;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Assets.HotReload;


[UnmanagedResource]
internal unsafe partial struct AssetFileWatcher
{
    private Inline2<ManagedResource<FileSystemWatcher>> Watchers;

    private static AssetSystem* AssetSystem;

    [System(SystemStage.Init)]
    public static void Init(in AssetSystem assetSystem, AssetFileWatcher* watcher, IFileSystem fileSystem, UnmanagedResourceRegistry registry)
    {
        AssetSystem = registry.GetResourcePointer<AssetSystem>();

        InitFileWatcher(out watcher->Watchers[0], fileSystem.GetPath(FilePathType.Engine));
        InitFileWatcher(out watcher->Watchers[1], fileSystem.GetPath(FilePathType.Content));


        static void InitFileWatcher(out ManagedResource<FileSystemWatcher> watcher, string path)
        {
            var fileSystemWatcher = new FileSystemWatcher(path);
            fileSystemWatcher.Filter = "*.kbin";
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Changed += (sender, args) => AssetSystem->AssetChanged(Path.GetRelativePath(path, args.FullPath));
            fileSystemWatcher.EnableRaisingEvents = true;
            watcher = ManagedResource<FileSystemWatcher>.Alloc(fileSystemWatcher);
        }
    }

    [System]
    public static void Update(EventWriter writer)
    {

    }
    [System(SystemStage.Shutdown)]
    public static void Shutdown(AssetFileWatcher* watcher)
    {
        Dispose(ref watcher->Watchers[0]);
        Dispose(ref watcher->Watchers[1]);

        static void Dispose(ref ManagedResource<FileSystemWatcher> watcher)
        {
            watcher.Value.Dispose();
            watcher.Dispose();
            watcher = default;
        }
    }
}
