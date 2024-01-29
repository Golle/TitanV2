using Titan.Application;
using Titan.Core.Logging;
using Titan.Windows;

namespace Titan.Modules;

public class ApplicationModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        Logger.Trace<ApplicationModule>($"Application Type = {config.ApplicationType}");
        if (config.ApplicationType == ApplicationType.Game)
        {
            builder.AddModule<WindowModule>();
        }

        return true;
    }

    public static bool Init(IApp app) => true;
    public static bool Shutdown(IApp app) => true;
}
