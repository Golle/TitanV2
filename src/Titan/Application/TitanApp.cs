using System.Collections.Frozen;
using System.Collections.Immutable;
using Titan.Application.Configurations;
using Titan.Application.Services;
using Titan.Core.Logging;
using Titan.Runners;

namespace Titan.Application;

internal sealed class TitanApp(FrozenDictionary<Type, ServiceDescriptor> services, ImmutableArray<Module> modules, ImmutableArray<ConfigurationDescriptor> configurations, IRunner runner) : IApp
{
    public T GetService<T>() where T : class, IService
        => services[typeof(T)].As<T>();

    public T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>
        => GetService<IConfigurationSystem>().GetConfigOrDefault<T>();

    public void UpdateConfig<T>(T config) where T : IConfiguration =>
        GetService<IConfigurationSystem>().UpdateConfig(config);

    public ImmutableArray<ConfigurationDescriptor> GetConfigurations()
        => configurations;

    public void Run()
    {
        Logger.Info<TitanApp>("Application starting");
        try
        {
            RunInternal();
        }
        finally
        {
            Logger.Info<TitanApp>("Application shutting down");
        }
    }

    private void RunInternal()
    {
        foreach (var module in modules)
        {
            if (!module.Init(this))
            {
                Logger.Error<TitanApp>($"Failed to init module. Name = {module.Name} Type = {module.Type}");
                return;
            }
        }
        Logger.Trace<TitanApp>($"Using runner {runner.GetType().Name}");
        runner.Init(this);

        while (runner.RunOnce())
        {
        }

        foreach (var module in modules.Reverse())
        {
            Logger.Trace<TitanApp>($"Shutdown module. Name = {module.Name}");
            if (!module.Shutdown(this))
            {
                Logger.Warning<TitanApp>($"Failed to shutdown module. Name = {module.Name} Type = {module.Type}");
            }
        }
    }
}
