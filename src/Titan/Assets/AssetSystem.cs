using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Core.Threading;
using Titan.IO.FileSystem;
using Titan.Resources;
using Titan.Services;
using Titan.Systems;

namespace Titan.Assets;

[UnmanagedResource]
internal unsafe partial struct AssetSystem
{
    public Inline8<AssetRegistry> Registers;
    public Inline16<AssetLoaderDescriptor> Loaders;
    public uint NumberOfRegisters;

    public TitanArray<Asset> Assets;
    public TitanArray<AssetDependency> Dependencies;

    public ManagedResource<IFileSystem> FileSystem;
    public GeneralAllocator Allocator;

    public TitanBuffer LoadersData;

    public readonly ReadOnlySpan<AssetRegistry> GetRegistries()
        => Registers.AsReadOnlySpan()[..(int)NumberOfRegisters];
    public bool SetRegisterAndLoaders(IReadOnlyList<AssetRegistryDescriptor> assetRegistries, IReadOnlyList<AssetLoaderDescriptor> assetLoaders)
    {
        for (var i = 0; i < assetRegistries.Count; ++i)
        {
            Registers[i].Descriptor = assetRegistries[i];
        }

        foreach (var loader in assetLoaders)
        {
            Loaders[loader.AssetId] = loader;
        }

        NumberOfRegisters = (uint)assetRegistries.Count;
        return true;
    }

    [System(SystemStage.Startup)]
    public static void Startup(AssetSystem* system, IFileSystem fileSystem, IMemoryManager memoryManager, IConfigurationManager configurationManager, UnmanagedResourceRegistry unmanagedResources, ServiceRegistry services)
    {
        var config = configurationManager.GetConfigOrDefault<AssetsConfig>();
        // Pre-calculate the memory usage for assets, loaders and dependencies so we can have all in the same block.

        if (!InitSystem(system, config, memoryManager))
        {
            Logger.Error<AssetSystem>($"Failed to initialize the {nameof(AssetSystem)}.");
            return;
        }

        // get the start of the dependencies array
        var dependencies = system->Dependencies.AsPointer();
        Logger.Trace<AssetSystem>($"Init the AssetSystem. Registries = {system->NumberOfRegisters}. Max AssetCount = {system->Assets.Length}");
        for (var i = 0; i < system->NumberOfRegisters; ++i)
        {
            var register = system->Registers.AsPointer() + i;

            var filePathType = register->EngineRegistry ? FilePathType.Engine : FilePathType.Content;
            var handle = fileSystem.Open(register->GetFilePath(), filePathType);
            if (handle.IsInvalid())
            {
                Logger.Error<AssetSystem>($"Failed to open the file at path {register->GetFilePath()}");
                continue;
            }
            register->File.Handle = handle;
            register->File.Size = fileSystem.GetLength(handle);

            var assetDescriptors = register->GetAssetDescriptors();
            foreach (ref readonly var descriptor in assetDescriptors)
            {
                var assetDependencies = register->GetDependencies(descriptor);
                system->Assets[descriptor.Id] = new()
                {
                    Descriptor = MemoryUtils.AsPointer(descriptor),
                    File = &register->File,
                    System = system,
                    Registry = register,
                    FileBuffer = null,
                    State = AssetState.Unloaded,
                    Dependencies = dependencies, // store dependencies pointer on the asset (regardless if it's empty)
                    NumberOfDependencies = (byte)assetDependencies.Length
                };

                // For each dependency, write the ID and move the pointer.
                foreach (var dependency in assetDependencies)
                {
                    var assetId = assetDescriptors[(int)dependency].Id;
                    *dependencies = new(system->Assets.GetPointer(assetId));
                    dependencies++;
                }
            }
        }

        //NOTE(Jens): Not the best way, but we need to use the file system.  Stretch: Replace IFileSystem with a struct, similar to AssetManager and EntityManager.
        system->FileSystem = ManagedResource<IFileSystem>.Alloc(fileSystem);

        // Init the Loaders
        var context = system->LoadersData.AsPointer();
        foreach (ref var loader in system->Loaders)
        {
            if (loader.AssetId == 0) // an empty slot.
            {
                continue;
            }

            loader.Context = context;
            context += loader.Size;
            if (!loader.Init(new AssetLoaderInitializer(unmanagedResources, services, system->GetRegistries())))
            {
                Logger.Error<AssetSystem>($"Failed to init the {loader.Name.GetString()} asset loader.");
            }
        }
    }

    /// <summary>
    /// Allocates all memory needed for the system
    /// </summary>
    /// <returns>True on success</returns>
    private static bool InitSystem(AssetSystem* system, AssetsConfig config, IMemoryManager memoryManager)
    {
        var (assetCount, dependencyCount) = GetMaxIndexAndDependencyCount(system->Registers.AsReadOnlySpan()[..(int)system->NumberOfRegisters]);

        if (!memoryManager.TryAllocArray(out system->Assets, assetCount + 1))
        {
            Logger.Error<AssetSystem>($"Failed to allocate an array for the assets. Size = {sizeof(Asset) * (assetCount + 1)} bytes");
            return false;
        }

        if (dependencyCount > 0)
        {
            //NOTE(Jens): We only allocate an array if there are dependencies.
            if (!memoryManager.TryAllocArray(out system->Dependencies, dependencyCount))
            {
                Logger.Error<AssetSystem>($"Failed to allocate an array for the asset dependencies. Size = {sizeof(AssetId) * dependencyCount} bytes");
                return false;
            }
        }
        else
        {
            system->Dependencies = TitanArray<AssetDependency>.Empty;
        }


        Logger.Trace<AssetSystem>($"File buffer size {config.FileBufferMaxSize} bytes");
        if (!memoryManager.TryCreateGeneralAllocator(out system->Allocator, config.FileBufferMaxSize))
        {
            Logger.Error<AssetSystem>($"Failed to create a allocator. Max Size = {config.FileBufferMaxSize} bytes");
            return false;
        }

        var (loadersSize, highestAssetId) = CalculateLoaderSizeAndAssetId(system->Loaders);
        Debug.Assert(highestAssetId < system->Loaders.Size);

        if (!memoryManager.TryAllocBuffer(out system->LoadersData, loadersSize))
        {
            Logger.Error<AssetSystem>($"Failed to allocate memory for the AssetLoaders. Size = {loadersSize} bytes");
            return false;
        }

        return true;

        static (uint MaxId, uint DependencyCount) GetMaxIndexAndDependencyCount(ReadOnlySpan<AssetRegistry> registries)
        {
            var maxId = 0u;
            var dependencies = 0u;
            foreach (ref readonly var registry in registries)
            {
                var descriptors = registry.GetAssetDescriptors();
                foreach (ref readonly var descriptor in descriptors)
                {
                    maxId = Math.Max(descriptor.Id, maxId);
                    dependencies += (uint)registry.GetDependencies(descriptor).Length;
                }
            }
            return (maxId, dependencies);
        }

        static (uint Size, uint AssetId) CalculateLoaderSizeAndAssetId(ReadOnlySpan<AssetLoaderDescriptor> loaders)
        {
            var assetId = 0u;
            var size = 0u;
            foreach (ref readonly var loader in loaders)
            {
                size += loader.Size;
                assetId = Math.Max(loader.AssetId, assetId);
            }
            return (size, assetId);
        }
    }


    [System(SystemStage.PostUpdate)]
    public static void Update(AssetSystem* system, IJobSystem jobSystem)
    {
        var count = system->Assets.Length;

        for (var i = 0; i < count; ++i)
        {
            var state = system->Assets.GetPointer(i);
            switch (state->State)
            {
                // Ignore all these states, they are handled by async tasks
                case AssetState.Loaded:
                case AssetState.Unloaded:
                case AssetState.CreatingResource:
                case AssetState.ReadingFile:
                    break;

                case AssetState.UnloadRequested:
                    state->State = AssetState.Unloading;
                    state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&UnloadResourceAsync, state));
                    break;

                case AssetState.LoadRequested when state->Descriptor->File.IsEmpty(): // Special case when there's no file data.
                case AssetState.ReadingFileCompleted:
                    if (HasPendingDependencies(state))
                    {
                        state->State = AssetState.ResolvingDependencies;
                    }
                    else
                    {
                        state->State = AssetState.CreatingResource;
                        state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&CreateResourceAsync, state));
                    }

                    break;

                case AssetState.LoadRequested:
                    var fileSize = GetFilesizeFromState(state);
                    state->FileBuffer = system->Allocator.Alloc(fileSize, false);
                    if (state->FileBuffer == null)
                    {
                        Logger.Warning("Failed to allocate memory for the file buffer.");
                        break;
                    }
                    state->State = AssetState.ReadingFile;
                    state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&ReadFileAsync, state));
                    break;

                case AssetState.ResolvingDependencies:
                    if (!HasPendingDependencies(state))
                    {
                        state->State = AssetState.CreatingResource;
                        state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&CreateResourceAsync, state));
                    }
                    break;

                case AssetState.ResourceCreated:
                    if (state->FileBuffer != null)
                    {
                        system->Allocator.Free(state->FileBuffer);
                        state->FileBuffer = null;
                    }
                    state->State = AssetState.Loaded;

                    break;


#if HOT_RELOAD_ASSETS
                case AssetState.Reload:
                    ReloadAsset(system, state);
                    break;
#endif
            }
        }

        static bool HasPendingDependencies(Asset* asset)
        {
            foreach (ref readonly var dependency in asset->GetDependencies())
            {
                if (!dependency.IsLoaded())
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static uint GetFilesizeFromState(Asset* state)
    {
#if HOT_RELOAD_ASSETS
        // We read the file size, since it might have changed.
        var fileSystem = state->System->FileSystem.Value;
        var handle = fileSystem.Open(state->Descriptor->File.BinaryAssetPath.GetString(), state->Registry->EngineRegistry ? FilePathType.Engine : FilePathType.Content);
        state->FileSize = (uint)fileSystem.GetLength(handle);
        fileSystem.Close(ref handle);
        return state->FileSize;
#else
        return state->Descriptor->File.Length;
#endif
    }

    public static void UnloadResourceAsync(Asset* asset)
    {
        Logger.Warning<AssetSystem>("Unload has not been implemented yet. We need reference counting for this to work properly.");
    }

    private static void ReadFileAsync(Asset* asset)
    {
#if DEBUG
        Logger.Trace<AssetSystem>($"Reading file {asset->Descriptor->File.AssetPath.GetString()}");
#endif
        var fileSystem = asset->System->FileSystem.Value;
        ref readonly var fileDescriptor = ref asset->Descriptor->File;

#if HOT_RELOAD_ASSETS
        var handle = fileSystem.Open(fileDescriptor.BinaryAssetPath.GetString(), asset->Registry->EngineRegistry ? FilePathType.Engine : FilePathType.Content);
        var bufferSpan = new Span<byte>(asset->FileBuffer, (int)asset->FileSize);
        var bytesRead = fileSystem.Read(handle, bufferSpan);
        fileSystem.Close(ref handle);
        if (asset->FileSize != bytesRead)
        {
            Logger.Warning<AssetSystem>($"Mismatch in bytes read and size of asset. Asset Size = {asset->FileSize} Bytes Read = {bytesRead}");
        }
        asset->State = AssetState.ReadingFileCompleted;
#else
        var bufferSpan = new Span<byte>(asset->FileBuffer, (int)fileDescriptor.Length);
        var bytesRead = fileSystem.Read(asset->File->Handle, bufferSpan, fileDescriptor.Offset);
        if (fileDescriptor.Length != bytesRead)
        {
            Logger.Warning<AssetSystem>($"Mismatch in bytes read and size of asset. Asset Size = {fileDescriptor.Length} Bytes Read = {bytesRead}");
        }
        asset->State = AssetState.ReadingFileCompleted;
#endif
    }

    private static void CreateResourceAsync(Asset* asset)
    {
        var loader = asset->GetLoader();
        Debug.Assert(loader != null);
        Debug.Assert(loader->Context != null, $"The context of the loader is null. Did you forget to register the loader? Type = {asset->Descriptor->Type}");

#if HOT_RELOAD_ASSETS
        var length = asset->FileSize;
#else
        var length = asset->Descriptor->File.Length;
#endif
        var buffer = new TitanBuffer(asset->FileBuffer, length);
        asset->Resource = loader->Load(*asset->Descriptor, buffer, asset->GetDependencies());

        if (asset->Resource == null)
        {
            asset->State = AssetState.Error;
        }

        asset->State = AssetState.ResourceCreated;
    }

    [System(SystemStage.EndOfLife)]
    public static void Shutdown(AssetSystem* system, IFileSystem fileSystem, UnmanagedResourceRegistry unmanagedResources, ServiceRegistry services, IMemoryManager memoryManager)
    {
        var initializer = new AssetLoaderInitializer(unmanagedResources, services, system->GetRegistries()); //TODO(Jens): Rename this struct.
        foreach (ref var loader in system->Loaders.AsSpan())
        {
            if (loader.Context == null)
            {
                continue;
            }
            loader.Shutdown(initializer);
        }

        for (var i = 0; i < system->NumberOfRegisters; ++i)
        {
            ref var registry = ref system->Registers[i];
            if (registry.File.Handle.IsValid())
            {
                fileSystem.Close(ref registry.File.Handle);
            }
        }
        system->FileSystem.Release();

        memoryManager.FreeBuffer(ref system->LoadersData);
        memoryManager.FreeArray(ref system->Assets);
        if (system->Dependencies.IsValid)
        {
            memoryManager.FreeArray(ref system->Dependencies);
        }
    }



    // This part is only used when Hot Reload is enabled, and none of these methods can be called in Release builds.
#if HOT_RELOAD_ASSETS
    private static void ReloadAsset(AssetSystem* system, Asset* asset)
    {
        if (!asset->GetDependencies().IsEmpty)
        {
            Logger.Warning<AssetSystem>("Asset Reloading is not supported for assets that has dependencies at this time.");
            asset->State = AssetState.Loaded;

            return;
        }

        var loader = asset->GetLoader();
        // buffer
        var fileSystem = system->FileSystem.Value;
        var handle = fileSystem.Open(asset->Descriptor->File.BinaryAssetPath.GetString(), asset->Registry->EngineRegistry ? FilePathType.Engine : FilePathType.Content);
        var fileLength = fileSystem.GetLength(handle);
        var buffer = system->Allocator.AllocBuffer((uint)fileLength);
        if (buffer.IsValid)
        {
            var bytesRead = fileSystem.Read(handle, buffer);
            if (bytesRead != fileLength)
            {
                Logger.Warning<AssetSystem>($"The bytes read and the file length are different. Read = {bytesRead} FileLength = {fileLength}");
            }


            if (!loader->Reload(asset->Resource, *asset->Descriptor, buffer.Slice(0, (uint)bytesRead)))
            {
                Logger.Error<AssetSystem>("Failed to reload the Asset, returned false.");
            }

            system->Allocator.FreeBuffer(ref buffer);
        }
        else
        {
            Logger.Error<AssetSystem>("failed to allocate a buffer for the file. Can't reload asset.");
        }
        fileSystem.Close(ref handle);
        // always set it back to loaded. might crash, but we'll live with it :)
        asset->State = AssetState.Loaded;
    }


    private static readonly Lock _lock = new();
    public void AssetChanged(string relativePath)
    {
        // we only want a single file watcher to call this at one time, try to avoid race conditions.
        lock (_lock)
        {
            //foreach (ref var asset in Assets.AsSpan())
            for (var i = 0; i < Assets.Length; ++i)
            {
                ref var asset = ref Assets[i];
                if (asset.State != AssetState.Loaded)
                {
                    // no reason to rigger reload on unloaded assets.
                    continue;
                }

                if (asset.Descriptor->File.BinaryAssetPath.GetString().Equals(relativePath, StringComparison.CurrentCultureIgnoreCase))
                {
                    asset.State = AssetState.Reload;
                    return;
                }
            }
        }
    }

#endif
}
