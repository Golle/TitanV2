using Titan.Core;

namespace Titan.Assets;

public interface IAssetLoader
{
    static abstract AssetLoaderDescriptor CreateDescriptor();
}

public unsafe interface IAssetLoader<T> : IAssetLoader where T : unmanaged, IAsset
{
    bool Init(in AssetLoaderInitializer init);
    void Shutdown(in AssetLoaderInitializer init);
    T* Load(in AssetDescriptor descriptor, TitanBuffer buffer, ReadOnlySpan<AssetDependency> dependencies);
    void Unload(T* asset);
    bool Reload(T* asset, in AssetDescriptor descriptor, TitanBuffer buffer);
}
