using Titan.Application;
using Titan.Core.IO.Platform;
using Titan.Core.Logging;
using Titan.Core.Memory.Platform;
using Titan.Core.Threading.Platform;
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

        builder
            .AddModule<FileSystemModule<Win32FileApi>>()

            .AddModule<MemoryModule<Win32PlatformAllocator>>()
            .AddModule<ThreadingModule<Win32NativeThreadApi>>()
            
            ;

        builder
            .AddModule<EventsModule>()
            .AddModule<ConfigurationsModule>()
            .AddModule<ResourcesModule>()
            .AddModule<ECSModule>()
            .AddModule<SystemsModule>();


        return true;
    }

    public static bool Init(IApp app)
        => true;

    public static bool Shutdown(IApp app)
        => true;
}
