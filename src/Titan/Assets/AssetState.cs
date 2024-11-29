namespace Titan.Assets;

internal enum AssetState
{
    Unloaded = 0,
    Loaded,
    LoadRequested,
    ReadingFile,
    ReadingFileCompleted,
    ResolvingDependencies,
    CreatingResource,
    ResourceCreated,
    UnloadRequested,
    Unloading,
#if HOT_RELOAD_ASSETS
    Reload,
#endif
    Error
}
