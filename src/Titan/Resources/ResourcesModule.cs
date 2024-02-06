using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Resources;

internal class ResourcesModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddService<IUnmanagedResources, UnmanagedResourceRegistry>(new UnmanagedResourceRegistry());
        return true;
    }

    public static bool Init(IApp app)
    {
        var registry = app.GetService<UnmanagedResourceRegistry>();
        var memoryManager = app.GetService<IMemoryManager>();
        var resources = app.GetResources();

        if (!registry.Init(memoryManager, resources))
        {
            Logger.Error<ResourcesModule>($"Failed to init {nameof(UnmanagedResourceRegistry)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<UnmanagedResourceRegistry>()
            .Shutdown();

        return true;
    }
}
