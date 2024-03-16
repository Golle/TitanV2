using Titan.Core;
using Titan.IO.FileSystem;
using Titan.Resources;

namespace Titan.Assets;

[UnmanagedResource]
internal unsafe partial struct AssetsContext
{
    public Inline8<AssetRegistry> Registries;
    public uint NumberOfRegistries;
}


internal unsafe struct AssetRegistry
{
    public AssetRegistryDescriptor Descriptor;
    public AssetFile File;
}

internal struct AssetFile
{
    public FileHandle Handle;
    public long Size;
}
