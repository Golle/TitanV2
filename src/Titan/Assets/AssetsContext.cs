using Titan.Core;
using Titan.Core.Memory.Allocators;
using Titan.IO.FileSystem;
using Titan.Resources;

namespace Titan.Assets;

[UnmanagedResource]
internal unsafe partial struct AssetsContext
{
    public Inline8<AssetRegistry> Registers;
    public Inline16<AssetLoaderDescriptor> Loaders;
    public uint NumberOfRegisters;

    public TitanArray<Asset> Assets;
    public ManagedResource<IFileSystem> FileSystem;
    public GeneralAllocator Allocator;

    public TitanBuffer LoadersInstances;
    
}
