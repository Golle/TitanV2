using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.IO.FileSystem;
using Titan.Systems;

namespace Titan.Assets;

internal unsafe partial struct AssetSystem
{

    [System(SystemStage.Init)]
    public static void Init(AssetsContext* context, IFileSystem fileSystem)
    {
        for (var i = 0; i < context->NumberOfRegistries; ++i)
        {
            ref var registry = ref context->Registries[i];
            var filePathType = registry.Descriptor.EngineRegistry ? FilePathType.Engine : FilePathType.Content;

            var handle = fileSystem.Open(registry.Descriptor.GetFilePath(), filePathType);
            if (handle.IsInvalid())
            {
                Logger.Error<AssetSystem>($"Failed to open the file at path {registry.Descriptor.GetFilePath()}");
                continue;
            }

            var size = fileSystem.GetLength(handle);
            registry.File = new AssetFile
            {
                Handle = handle,
                Size = size
            };
        }

    }

    [System(SystemStage.PostUpdate)]
    public static void Update(AssetsContext* context)
    {


    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(AssetsContext* context, IFileSystem fileSystem)
    {
        for (var i = 0; i < context->NumberOfRegistries; ++i)
        {
            ref var registry = ref context->Registries[i];
            if (registry.File.Handle.IsValid())
            {
                fileSystem.Close(ref registry.File.Handle);
            }
        }
    }
}
