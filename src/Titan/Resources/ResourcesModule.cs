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
        var memorySystem = app.GetService<IMemorySystem>();
        var resources = app.GetResources();

        if (!registry.Init(memorySystem, resources))
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
