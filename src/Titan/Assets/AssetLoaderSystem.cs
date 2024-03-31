using Titan.Core.Logging;
using Titan.Resources;
using Titan.Services;
using Titan.Systems;

namespace Titan.Assets;

internal unsafe partial struct AssetLoaderSystem
{
    [System(SystemStage.Init)]
    public static void Init(in AssetsContext context, UnmanagedResourceRegistry unmanagedResources, ServiceRegistry services)
    {
        var initializer = new AssetLoaderInitializer(unmanagedResources, services);
        var loaders = new Span<AssetLoaderDescriptor>(context.Loaders.AsPointer(), context.Loaders.Size);
        foreach (ref var loader in loaders)
        {
            if (loader.Context == null)
            {
                continue;
            }

            if (!loader.Init(initializer))
            {
                Logger.Error<AssetLoaderSystem>($"Failed to init the {loader.Name.GetString()} asset loader.");
            }
        }
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(in AssetsContext context)
    {
        var loaders = context.Loaders.AsPointer();
        var size = context.Loaders.Size;
        for (var i = 0; i < size; ++i)
        {
            if (loaders[i].Context == null)
            {
                continue;
            }

            loaders[i].Shutdown();
        }
    }
}
