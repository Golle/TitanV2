namespace Titan.Assets;

internal unsafe struct AssetRegistryDescriptor(RegistryId id, bool engineRegistry)
{
    public readonly RegistryId Id = id;
    public readonly bool EngineRegistry = engineRegistry;

    public delegate*<ReadOnlySpan<char>> GetFilePath;
    public delegate*<ReadOnlySpan<AssetDescriptor>> GetAssetDescriptors;
    public delegate*<in AssetDescriptor, ReadOnlySpan<uint>> GetDependencies;

    public static AssetRegistryDescriptor Create<T>(bool engineRegistry) where T : unmanaged, IAssetRegistry =>
        new(T.Id, engineRegistry)
        {
            GetAssetDescriptors = &T.GetAssetDescriptors,
            GetDependencies = &T.GetDependencies,
            GetFilePath = &T.GetFilePath
        };
}
