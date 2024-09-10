using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core.Logging;

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
public readonly unsafe struct AssetsManager
{
    private readonly AssetSystem* _assetSystem;
    internal AssetsManager(AssetSystem* assetSystem) => _assetSystem = assetSystem;

    public AssetHandle<T> Load<T>(in AssetDescriptor descriptor) where T : unmanaged, IAsset
    {
        Debug.Assert(T.Type == descriptor.Type, $"Trying to load asset of type {descriptor.Type} but treated as {T.Type}");

        ref var asset = ref _assetSystem->Assets[descriptor.Id];
        if (asset.State == AssetState.Unloaded)
        {
            asset.State = AssetState.LoadRequested;
            LoadDependencies(descriptor);
        }

        return new AssetHandle<T>(descriptor.Id);
    }

    private void LoadDependencies(in AssetDescriptor descriptor)
    {
        if (descriptor.Dependencies.Count == 0)
        {
            return;
        }

        //NOTE(Jens): A lot of code for almost nothing. See if there are better ways to do this.
        var asset = _assetSystem->Assets.GetPointer(descriptor.Id);
        var dependencies = asset->Registry->GetDependencies(descriptor);
        Debug.Assert(dependencies.Length > 0);
        var assetDescriptors = asset->Registry->GetAssetDescriptors();
        foreach (var dependencyIndex in dependencies)
        {
            var dependencyId = assetDescriptors[(int)dependencyIndex].Id;
            var dependencyAsset = _assetSystem->Assets.GetPointer(dependencyId);
            if (dependencyAsset->State == AssetState.Unloaded)
            {
                dependencyAsset->State = AssetState.LoadRequested;
            }
        }
    }

    public void Unload<T>(ref AssetHandle<T> handle) where T : unmanaged, IAsset
    {
        Debug.Assert(handle.IsValid);

        handle = default;
    }

    public ref readonly T Get<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset
    {
        Debug.Assert(handle.IsValid);
        return ref *(T*)_assetSystem->Assets[handle.Index].Resource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsLoaded<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset
        => _assetSystem->Assets[handle.Index].State == AssetState.Loaded;

    /// <summary>
    /// Loads the resource immediately on the current thread.
    /// <remarks>This function can only be used during Init system stage.</remarks>
    /// </summary>
    /// <param name="descriptor">The asset to load</param>
    /// <returns>The handle to the asset</returns>
    public AssetHandle<T> LoadImmediately<T>(in AssetDescriptor descriptor) where T : unmanaged, IAsset
    {
        Debug.Assert(descriptor.Dependencies.Count == 0, "Can't load assets immediately that have dependencies.");
        //TODO(Jens): Add check for current state
        //TODO(Jens): This requires a lock, since multiple threads can call this function (Init is async)
        
        var asset = _assetSystem->Assets.GetPointer(descriptor.Id);

        //TODO(Jens): Rework this lock, but for now this will be good enough :) this will prevent any async loading from happening at startup, which is not what we want.
        //NOTE(Jens): no support for dependencies.
        lock (_lock)
        {
            if (asset->State == AssetState.Loaded)
            {
                return new AssetHandle<T>(descriptor.Id);
            }
            if (asset->State != AssetState.Unloaded)
            {
                Logger.Error<AssetsManager>("Trying to load an asset that's in the wrong state.");
                return AssetHandle<T>.Invalid;
            }
            var fileSystem = _assetSystem->FileSystem.Value;
            var fileBuffer = _assetSystem->Allocator.AllocBuffer(descriptor.File.Length);
            try
            {
                var bytesRead = fileSystem.Read(asset->File->Handle, fileBuffer.AsSpan(), descriptor.File.Offset);
                if (descriptor.File.Length != bytesRead)
                {
                    Logger.Warning<AssetsManager>($"Mistmatch in bytes read. Expected = {descriptor.File.Length} bytes, read = {bytesRead} bytes");
                }

                //NOTE(Jens): We do a Slice here because the buffer returned by the allocator might be bigger.
                asset->Resource = asset->GetLoader()->Load(descriptor, fileBuffer.Slice(0, descriptor.File.Length), ReadOnlySpan<AssetDependency>.Empty);
                if (asset->Resource == null)
                {
                    Logger.Error<AssetsManager>("Failed to load resource");
                    return AssetHandle<T>.Invalid;
                }
                asset->State = AssetState.Loaded;
            }
            finally
            {
                _assetSystem->Allocator.FreeBuffer(ref fileBuffer);
            }
        }

        return new AssetHandle<T>(descriptor.Id);
    }

    private static readonly object _lock = new();
}
