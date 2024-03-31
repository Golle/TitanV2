using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Assets;

internal sealed unsafe class AssetsManager : IAssetsManager
{
    private UnmanagedResource<AssetsContext> _context;
    private IMemoryManager? _memoryManager;
    public bool Init(IReadOnlyList<AssetRegistryDescriptor> registers, UnmanagedResource<AssetsContext> assetsContext, IReadOnlyList<AssetLoaderDescriptor> assetLoaders, IMemoryManager memoryManager)
    {
        _context = assetsContext;
        ref var context = ref assetsContext.AsRef;
        context = default;

        // Init the registers
        Logger.Trace<AssetsManager>($"Asset registers created. Count = {context.NumberOfRegisters}");
        Debug.Assert(registers.Count < context.Registers.Size);
        for (var i = 0; i < registers.Count; ++i)
        {
            context.Registers[i].Descriptor = registers[i];
        }
        context.NumberOfRegisters = (uint)registers.Count;

        if (assetLoaders.Count == 0)
        {
            Logger.Warning<AssetsManager>("No asset loaders have been registered.");
            return true;
        }
        // Init the loaders
        var loadersSize = assetLoaders.Sum(static a => a.Size);
        var highestAssetId = assetLoaders.Max(static a => a.AssetId);
        Debug.Assert(highestAssetId < context.Loaders.Size);

        if (!memoryManager.TryAllocBuffer(out context.LoadersInstances, (uint)loadersSize))
        {
            Logger.Error<AssetsManager>($"Failed to allocate memory for the AssetLoaders. Size = {loadersSize} bytes");
            return false;
        }

        var offset = 0u;
        foreach (var descriptor in assetLoaders)
        {
            ref var loader = ref context.Loaders[descriptor.AssetId];
            loader = descriptor;
            loader.Context = context.LoadersInstances.AsPointer() + offset;
            offset += descriptor.Size;
        }

        _memoryManager = memoryManager;
        return true;
    }

    public void Shutdown()
    {
        ref var context = ref _context.AsRef;
        _memoryManager?.FreeBuffer(ref context.LoadersInstances);
        context = default;
    }

    public AssetHandle<T> Load<T>(in AssetDescriptor descriptor) where T : unmanaged, IAsset
    {
        ref var context = ref _context.AsRef;
        ref var asset = ref context.Assets[descriptor.Id];
        if (asset.State == AssetState.Unloaded)
        {
            asset.State = AssetState.LoadRequested;
        }

        return new AssetHandle<T>(descriptor.Id);
    }

    public void Unload<T>(ref AssetHandle<T> handle) where T : unmanaged, IAsset
    {
        Debug.Assert(handle.IsValid);

        handle = default;
    }

    public bool IsLoaded<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset
        => _context.AsReadOnlyRef.Assets[handle.Index].State == AssetState.Loaded;

    public ref readonly T Get<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset
    {
        Debug.Assert(handle.IsValid);

        return ref Unsafe.NullRef<T>();
    }
}
