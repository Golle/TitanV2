using System.Collections.Frozen;
using System.Collections.Immutable;
using Titan.Core.Logging;
using Titan.Windows.Win32;

namespace Titan.Application;

internal class TitanApp(FrozenDictionary<Type, IService> services, ImmutableArray<Module> modules, ImmutableArray<IConfiguration> configurations) : IApp
{
    public T GetService<T>() where T : IService
        => (T)services[typeof(T)];

    public T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>
        => (T?)configurations.FirstOrDefault(c => c.GetType() == typeof(T)) ?? T.Default;

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
            Logger.Trace<TitanApp>($"Init module. Name = {module.Name}");
            if (!module.Init(this))
            {
                Logger.Error<TitanApp>($"Failed to init module. Name = {module.Name} Type = {module.Type}");
                return;
            }
        }

        var window = GetService<IWindow>();


        while (window.UpdateBlocking())
        {
            //Thread.Sleep(1);
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
