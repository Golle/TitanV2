namespace Titan.Assets;

public interface IAssetRegistry
{
    static abstract RegistryId Id { get; }
    static abstract ReadOnlySpan<char> GetFilePath();
    static abstract ReadOnlySpan<AssetDescriptor> GetAssetDescriptors();
    static abstract ReadOnlySpan<uint> GetDependencies(in AssetDescriptor descriptor);
}
