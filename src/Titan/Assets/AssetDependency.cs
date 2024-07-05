using System.Diagnostics;
using Titan.Core;

namespace Titan.Assets;

/// <summary>
/// A type that has methods to access the dependencies of a loaded asset. Validates the Type.
/// </summary>
public readonly unsafe struct AssetDependency
{
    private readonly Asset* _asset;
    internal AssetDependency(Asset* asset) => _asset = asset;
    /// <summary>
    /// Returns a pointer to the created resource of the asset
    /// <remarks>This is not read only, so changed can be made to underlying asset. DONT DO IT!</remarks>
    /// </summary>
    /// <typeparam name="T">The type, will be validated</typeparam>
    /// <returns>Pointer to the resource</returns>
    public T* GetAssetPointer<T>() where T : unmanaged, IAsset
    {
        Debug.Assert(T.Type == _asset->Descriptor->Type);
        Debug.Assert(_asset->Resource != null);
        return (T*)_asset->Resource;
    }

    /// <summary>
    /// Returns the readonly reference to the resource of the asset
    /// </summary>
    /// <typeparam name="T">The type, will be validated</typeparam>
    /// <returns>Reference to the resource</returns>
    public ref readonly T GetAsset<T>() where T : unmanaged, IAsset => ref *GetAssetPointer<T>();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Handle<T> GetHandle<T>() where T : unmanaged, IAsset => _asset->Descriptor->Id.Value;


    /// <summary>
    /// Check if the asset is loaded.
    /// </summary>
    /// <returns>True if the asset is loaded.</returns>
    internal bool IsLoaded() => _asset->State == AssetState.Loaded;
}
