using Titan.Application;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core.IO.Platform;
using Titan.Core.Logging;
using Titan.Core.Memory.Platform;
using Titan.Core.Threading.Platform;
using Titan.ECS;
using Titan.Events;
using Titan.IO;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Modules;

internal class CoreModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Info<CoreModule>($"Welcome to Titan! Your current platform is {GlobalConfiguration.Platform}");

        if (GlobalConfiguration.Platform != Platforms.Windows)
        {
            throw new PlatformNotSupportedException($"Support for platform {GlobalConfiguration.Platform} has not been implemented yet.");
        }

        // Register app lifetime handling first.
        builder
            .AddResource<ApplicationLifetime>()
            .AddSystems<ApplicationLifetimeSystem>();

        // Platform specific modules
        builder
            .AddModule<FileSystemModule<Win32FileApi>>()
            .AddModule<MemoryModule<Win32PlatformAllocator>>()
            .AddModule<ThreadingModule<Win32NativeThreadApi>>()

            ;

        // Base modules, serivces and resources.
        builder
            .AddService<IConfigurationManager, ConfigurationManager>(new ConfigurationManager())
            .AddService(new UnmanagedResourceRegistry())
            .AddResource<SystemsScheduler>()
            .AddService(new EventSystem())
            .AddSystems<EventSystem>()
            .AddResource<EventState>()


            .AddModule<AssetsModule>()
            .AddModule<ECSModule>()
            ;


        return true;
    }
}
