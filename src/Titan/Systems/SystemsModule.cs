using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;
internal sealed class SystemsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder.AddService<ISystemsScheduler, SystemsScheduler>(new SystemsScheduler());
        return true;
    }

    public static bool Init(IApp app)
    {
        var scheduler = app.GetService<SystemsScheduler>();
        var unmanaged = app.GetService<IUnmanagedResources>();
        var managed = app.GetService<IManagedServices>();
        var memoryManager = app.GetService<IMemoryManager>();
        var systems = app.GetSystems();

        if (!scheduler.Init(memoryManager, systems, unmanaged, managed))
        {
            Logger.Error<SystemsModule>($"Failed to init the {nameof(SystemsScheduler)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        app.GetService<SystemsScheduler>()
            .Shutdown();

        return true;
    }
}
