using Titan.Application;
using Titan.Windows.Linux;
using Titan.Windows.Win32;

namespace Titan.Windows;

internal class WindowModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        if (GlobalConfiguration.Platform == Platforms.Windows)
        {
            builder.AddModule<Win32WindowModule>();
        }
        else if (GlobalConfiguration.Platform == Platforms.Linux)
        {
            builder.AddModule<LinuxWindowModule>();
        }
        else
        {
            throw new PlatformNotSupportedException($"The platform {GlobalConfiguration.Platform} is not supported.");
        }
        return true;
    }
}
