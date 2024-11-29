using System.Collections.Concurrent;
using Titan.Core;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Events;
using Titan.IO.FileSystem;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Assets.HotReload;

#if HOT_RELOAD_ASSETS
[UnmanagedResource]
internal unsafe partial struct AssetFileWatcher
{
    private Inline2<ManagedResource<FileSystemWatcher>> Watchers;
    // we use this to prevent double reloads
    private static readonly ConcurrentDictionary<string, DateTime> LastWriteTracker = new();
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
            fileSystemWatcher.Changed += (sender, args) =>
            {
                var updateTime = DateTime.Now;
                if (LastWriteTracker.TryGetValue(path, out var time) && (updateTime - time).TotalMilliseconds < 100)
                {
                    // we ignore duplicate events, or if the file wasn't changed.
                    return;
                }
                LastWriteTracker[path] = updateTime;
                AssetSystem->AssetChanged(Path.GetRelativePath(path, args.FullPath));
            };
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

#endif
