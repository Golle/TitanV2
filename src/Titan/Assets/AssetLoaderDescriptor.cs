using Titan.Core;
using Titan.Core.Strings;

namespace Titan.Assets;

public unsafe struct AssetLoaderDescriptor(
    uint size,
    StringRef name,
    uint assetId,
    delegate*<void*, in AssetDescriptor, TitanBuffer, ReadOnlySpan<AssetDependency>, void*> load,
    delegate*<void*, void*, void> unload,
    delegate*<void*, in AssetLoaderInitializer, bool> init,
    delegate*<void*, in AssetLoaderInitializer, void> shutdown)
{

    public readonly uint Size = size;
    public readonly StringRef Name = name;
    public readonly uint AssetId = assetId;

    public void* Context;
    public readonly void* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies)
        => load(Context, descriptor, buffer, dependencies);
    public readonly void Unload(void* asset)
        => unload(Context, asset);
    public readonly bool Init(in AssetLoaderInitializer initializer)
        => init(Context, initializer);
    public readonly void Shutdown(in AssetLoaderInitializer initializer)
        => shutdown(Context, initializer);
}
