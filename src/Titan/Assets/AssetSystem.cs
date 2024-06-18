using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.IO.FileSystem;
using Titan.Systems;

namespace Titan.Assets;

internal unsafe partial struct AssetSystem
{
    [System(SystemStage.PreInit)]
    public static void Init(AssetsContext* context, IFileSystem fileSystem, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var assetCount = GetMaxIndex(context->Registers.AsReadOnlySpan()[..(int)context->NumberOfRegisters]) + 1;
        if (!memoryManager.TryAllocArray(out context->Assets, assetCount))
        {
            Logger.Error<AssetSystem>($"Failed to allocate an array for the assets. Size = {sizeof(Asset) * assetCount} bytes");
        }

        var config = configurationManager.GetConfigOrDefault<AssetsConfig>();
        Logger.Trace<AssetSystem>($"File buffer size {config.FileBufferMaxSize} bytes");
        if (!memoryManager.TryCreateGeneralAllocator(out context->Allocator, config.FileBufferMaxSize))
        {
            Logger.Error<AssetSystem>($"Failed to create a allocator. Max Size = {config.FileBufferMaxSize} bytes");
        }

        Logger.Trace<AssetSystem>($"Init the AssetSystem. Registries = {context->NumberOfRegisters}. Max AssetCount = {assetCount}");
        for (var i = 0; i < context->NumberOfRegisters; ++i)
        {
            var register = context->Registers.AsPointer() + i;

            var filePathType = register->EngineRegistry ? FilePathType.Engine : FilePathType.Content;
            var handle = fileSystem.Open(register->GetFilePath(), filePathType);
            if (handle.IsInvalid())
            {
                Logger.Error<AssetSystem>($"Failed to open the file at path {register->GetFilePath()}");
                continue;
            }
            register->File.Handle = handle;
            register->File.Size = fileSystem.GetLength(handle);

            foreach (ref readonly var descriptor in register->GetAssetDescriptors())
            {
                context->Assets[descriptor.Id] = new()
                {
                    Descriptor = MemoryUtils.AsPointer(descriptor), //NOTE: this might crash , unless they are stored as constants.. We'll see :D Worst case is that we can copy them, should not use a massive amount of memory
                    File = &register->File,
                    Context = context,
                    FileBuffer = null,
                    State = AssetState.Unloaded
                };
            }
        }

        //NOTE(Jens): Not the best way, but we need to use the file system. 
        context->FileSystem = ManagedResource<IFileSystem>.Alloc(fileSystem);

        static uint GetMaxIndex(ReadOnlySpan<AssetRegistry> registries)
        {
            var maxId = 0u;
            foreach (ref readonly var registry in registries)
            {
                var descriptors = registry.GetAssetDescriptors();
                if (descriptors.Length == 0)
                {
                    continue;
                }
                maxId = Math.Max(descriptors[^1].Id, maxId);
            }

            return maxId;
        }
    }

    [System(SystemStage.PostUpdate)]
    public static void Update(AssetsContext* context, IJobSystem jobSystem)
    {
        var count = context->Assets.Length;

        for (var i = 0; i < count; ++i)
        {
            var state = context->Assets.GetPointer(i);
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
                case AssetState.LoadRequested:
                    state->FileBuffer = context->Allocator.Alloc(state->Descriptor->File.Length, false);
                    if (state->FileBuffer == null)
                    {
                        Logger.Warning("Failed to allocate memory for the file buffer.");
                        break;
                    }
                    state->State = AssetState.ReadingFile;
                    state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&ReadFileAsync, state));
                    break;

                case AssetState.ReadingFileCompleted:
                    state->State = AssetState.CreatingResource;
                    state->AsyncJobHandle = jobSystem.Enqueue(JobDescriptor.CreateTyped(&CreateResourceAsync, state));
                    break;

                case AssetState.ResourceCreated:
                    if (state->FileBuffer != null)
                    {
                        context->Allocator.Free(state->FileBuffer);
                        state->FileBuffer = null;
                    }
                    state->State = AssetState.Loaded;

                    break;
            }
        }
    }

    public static void UnloadResourceAsync(Asset* asset)
    {

    }

    private static void ReadFileAsync(Asset* asset)
    {
#if DEBUG
        Logger.Trace<AssetSystem>($"Reading file {asset->Descriptor->File.AssetPath.GetString()}");
#endif
        var fileSystem = asset->Context->FileSystem.Value;
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
        var resource = loader->Load(*asset->Descriptor, buffer);

        if (resource == null)
        {
            asset->State = AssetState.Error;
        }

        asset->State = AssetState.ResourceCreated;
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(AssetsContext* context, IFileSystem fileSystem)
    {
        for (var i = 0; i < context->NumberOfRegisters; ++i)
        {
            ref var registry = ref context->Registers[i];
            if (registry.File.Handle.IsValid())
            {
                fileSystem.Close(ref registry.File.Handle);
            }
        }
        context->FileSystem.Release();
    }
}
