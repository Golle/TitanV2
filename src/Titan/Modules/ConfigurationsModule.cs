using Titan.Application;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.IO.FileSystem;

namespace Titan.Modules;

internal sealed class ConfigurationsModule : IModule
{
    public static bool Build(IAppBuilder builder, AppConfig config)
    {
        var fileSystem = builder.GetService<IFileSystem>();
        builder.AddService<IConfigurationManager, ConfigurationManager>(new ConfigurationManager(fileSystem));

        return true;
    }

    public static bool Init(IApp app)
    {
        var system = app.GetService<ConfigurationManager>();
        var configurations = app.GetConfigurations();
        if (!system.Init(configurations))
        {
            Logger.Error<ConfigurationsModule>($"Failed to init {nameof(ConfigurationManager)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        var system = app.GetService<ConfigurationManager>();

        system.Shutdown();
        return true;
    }
}
