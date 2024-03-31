using System.Runtime.InteropServices;

namespace Titan.Assets;

public interface IAsset
{
    static abstract AssetType Type { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct AssetHandle<T>(int index) where T : unmanaged, IAsset
{
    public readonly int Index = index;
    public bool IsValid => Index != 0;
    public bool IsInvalid => Index == 0;

    public static readonly AssetHandle<T> Invalid = default;
}

/// <summary>
/// The AssetsManager lets you load, unload and access IAsset data that has been packed inside a binary.
/// <remarks>The assets manager should only be used in the Update phase of the systems. </remarks>
/// </summary>
public interface IAssetsManager : IService
{
    AssetHandle<T> Load<T>(in AssetDescriptor descriptor) where T : unmanaged, IAsset;
    void Unload<T>(ref AssetHandle<T> handle) where T : unmanaged, IAsset;
    ref readonly T Get<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset;
    bool IsLoaded<T>(in AssetHandle<T> handke) where T : unmanaged, IAsset;
}
