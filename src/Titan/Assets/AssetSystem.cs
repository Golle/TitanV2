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
            if (!loader.Init(new AssetLoaderInitializer(unmanagedResources, services)))
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
                    state->FileBuffer = system->Allocator.Alloc(state->Descriptor->File.Length, false);
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

        var bufferSpan = new Span<byte>(asset->FileBuffer, (int)fileDescriptor.Length);
        var bytesRead = fileSystem.Read(asset->File->Handle, bufferSpan, fileDescriptor.Offset);
        if (fileDescriptor.Length != bytesRead)
        {
            Logger.Warning<AssetSystem>($"Mismatch in bytes read and size of asset. Asset Size = {fileDescriptor.Length} Bytes Read = {bytesRead}");
        }
        asset->State = AssetState.ReadingFileCompleted;
    }

    private static void CreateResourceAsync(Asset* asset)
    {
        var loader = asset->GetLoader();
        Debug.Assert(loader != null);

        var buffer = new TitanBuffer(asset->FileBuffer, asset->Descriptor->File.Length);
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
        var initializer = new AssetLoaderInitializer(unmanagedResources, services); //TODO(Jens): Rename this struct.
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
}
