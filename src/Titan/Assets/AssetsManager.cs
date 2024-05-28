using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Assets;

internal sealed unsafe class AssetsManager : IAssetsManager
{
    private readonly object _lock = new();

    private UnmanagedResource<AssetsContext> _context;
    private IMemoryManager? _memoryManager;
    public bool Init(IReadOnlyList<AssetRegistryDescriptor> registers, UnmanagedResource<AssetsContext> assetsContext, IReadOnlyList<AssetLoaderDescriptor> assetLoaders, IMemoryManager memoryManager)
    {
        _context = assetsContext;
        ref var context = ref assetsContext.AsRef;
        context = default;

        // Init the registers

        Debug.Assert(registers.Count < context.Registers.Size);
        for (var i = 0; i < registers.Count; ++i)
        {
            context.Registers[i].Descriptor = registers[i];
        }
        context.NumberOfRegisters = (uint)registers.Count;
        Logger.Trace<AssetsManager>($"Asset registers created. Count = {context.NumberOfRegisters}");

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
        Debug.Assert(T.Type == descriptor.Type, $"Trying to load asset of type {descriptor.Type} but treated as {T.Type}");
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

    public AssetHandle<T> LoadImmediately<T>(in AssetDescriptor descriptor) where T : unmanaged, IAsset
    {
        //TODO(Jens): Add check for current state
        //TODO(Jens): This requires a lock, since multiple threads can call this function (Init is async)

        var context = _context.AsPointer;
        var asset = context->Assets.GetPointer(descriptor.Id);
        //TODO(Jens): Rework this lock, but for now this will be good enough :) this will prevent any async loading from happening at startup, which is not what we want.
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
            var fileSystem = context->FileSystem.Value;
            var fileBuffer = context->Allocator.AllocBuffer(descriptor.File.Length);
            try
            {
                var bytesRead = fileSystem.Read(asset->File->Handle, fileBuffer.AsSpan(), descriptor.File.Offset);
                if (descriptor.File.Length != bytesRead)
                {
                    Logger.Warning<AssetsManager>($"Mistmatch in bytes read. Expected = {descriptor.File.Length} bytes, read = {bytesRead} bytes");
                }

                //NOTE(Jens): We do a Slice here because the buffer returned by the allocator might be bigger.
                asset->Resource = asset->GetLoader()->Load(descriptor, fileBuffer.Slice(0, descriptor.File.Length));
                if (asset->Resource == null)
                {
                    Logger.Error<AssetsManager>("Failed to load resource");
                    return AssetHandle<T>.Invalid;
                }
                asset->State = AssetState.Loaded;
            }
            finally
            {
                context->Allocator.FreeBuffer(ref fileBuffer);
            }
        }

        return new AssetHandle<T>(descriptor.Id);
    }

    public ref readonly T Get<T>(in AssetHandle<T> handle) where T : unmanaged, IAsset
    {
        Debug.Assert(handle.IsValid);
        return ref *(T*)_context.AsRef.Assets[handle.Index].Resource;
    }
}
