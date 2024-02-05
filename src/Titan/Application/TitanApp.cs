using System.Collections.Immutable;
using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Resources;
using Titan.Runners;
using Titan.Services;

namespace Titan.Application;

internal sealed class TitanApp(IManagedServices services, ImmutableArray<ModuleDescriptor> modules, ImmutableArray<ConfigurationDescriptor> configurations, ImmutableArray<UnmanagedResourceDescriptor> resources, IRunner runner) : IApp, IRunnable
{
    public T GetService<T>() where T : class, IService
        => services.GetService<T>();

    public ManagedResource<T> GetServiceHandle<T>() where T : class, IService
        => services.GetHandle<T>();

    public T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>
        => GetService<IConfigurationSystem>().GetConfigOrDefault<T>();

    public void UpdateConfig<T>(T config) where T : IConfiguration =>
        GetService<IConfigurationSystem>().UpdateConfig(config);

    public ImmutableArray<ConfigurationDescriptor> GetConfigurations()
        => configurations;

    public ImmutableArray<UnmanagedResourceDescriptor> GetResources()
        => resources;

    public void Run()
    {
        Logger.Info<TitanApp>("Application starting");
        try
        {
            RunInternal();
        }
        catch (Exception e)
        {
            Logger.Error<TitanApp>($"An unahandled exception was thrown from the App. Type = {e.GetType().Name}. Message = {e.Message}");
            Logger.Error<TitanApp>(e.StackTrace ?? "[Stacktrace Missing]");
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

        Logger.Trace<TitanApp>("Init complete. Doing a GC Collect.");
        var gcTimer = Stopwatch.StartNew();
        GC.Collect();
        gcTimer.Stop();
        Logger.Trace<TitanApp>($"GC Collect completed in {gcTimer.Elapsed.TotalMilliseconds} ms");

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

