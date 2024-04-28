namespace Titan.Assets;

internal unsafe struct AssetRegistry
{
    public AssetRegistryDescriptor Descriptor;
    public AssetFile File;
    public readonly RegistryId Id => Descriptor.Id;
    public readonly bool EngineRegistry => Descriptor.EngineRegistry;
    public readonly ReadOnlySpan<char> GetFilePath() => Descriptor.GetFilePath();
    public readonly ReadOnlySpan<AssetDescriptor> GetAssetDescriptors() => Descriptor.GetAssetDescriptors();
    public readonly ReadOnlySpan<uint> GetDependencies(in AssetDescriptor descriptor) => Descriptor.GetDependencies(descriptor);
}
