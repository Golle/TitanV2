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
        builder.AddService<IConfigurationSystem, ConfigurationSystem>(new ConfigurationSystem(fileSystem));

        return true;
    }

    public static bool Init(IApp app)
    {
        var system = app.GetService<ConfigurationSystem>();
        var configurations = app.GetConfigurations();
        if (!system.Init(configurations))
        {
            Logger.Error<ConfigurationsModule>($"Failed to init {nameof(ConfigurationSystem)}");
            return false;
        }

        return true;
    }

    public static bool Shutdown(IApp app)
    {
        var system = app.GetService<ConfigurationSystem>();

        system.Shutdown();
        return true;
    }
}
