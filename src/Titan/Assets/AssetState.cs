namespace Titan.Assets;

internal enum AssetState
{
    Unloaded = 0,
    Loaded,
    LoadRequested,
    ReadingFile,
    ReadingFileCompleted,
    CreatingResource,
    ResourceCreated,
    UnloadRequested,
    Unloading,
    Error
}
