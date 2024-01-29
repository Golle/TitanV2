using Titan.Application;

namespace Titan.Windows.Linux;
internal class LinuxWindowModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config) => throw new PlatformNotSupportedException("Linux is not supported at this time");
    public static bool Init(IApp app) => throw new PlatformNotSupportedException("Linux is not supported at this time");
    public static bool Shutdown(IApp app) => throw new PlatformNotSupportedException("Linux is not supported at this time");
}
