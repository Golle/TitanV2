using Titan.Application;
using Titan.Core.Logging;
using Titan.Core.Memory.Platform;
using Titan.Core.Threading.Platform;

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
            .AddModule<MemoryModule<Win32PlatformAllocator>>()
            .AddModule<ThreadingModule<Win32NativeThreadApi>>();

        return true;
    }

    public static bool Init(IApp app)
        => true;

    public static bool Shutdown(IApp app)
        => true;
}