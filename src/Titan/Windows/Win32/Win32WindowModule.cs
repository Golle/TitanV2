using Titan.Application;

namespace Titan.Windows.Win32;

internal class Win32WindowModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        builder
            .AddResource<Win32MessageQueue>()
            .AddResource<Window>()
            .AddSystems<Win32WindowSystem>()
            ;

        return true;
    }
}
