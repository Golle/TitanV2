using System.Collections.Frozen;
using System.Collections.Immutable;
using Titan.Application.Configurations;
using Titan.Application.Services;
using Titan.Core.Logging;
using Titan.Runners;

namespace Titan.Application;

internal class AppBuilder(AppConfig appConfig) : IAppBuilder
{
    //NOTE(Jens): Dictionaries will be faster, but probably not worth it.
    private readonly List<ConfigurationDescriptor> _configurations = new();

    private readonly List<Module> _modules = new();
    private readonly Dictionary<Type, ServiceDescriptor> _services = new();
    private IRunner? _runner;

    public IAppBuilder AddService<T>(T instance) where T : class, IService
    {
        Logger.Trace<AppBuilder>($"Add Service {typeof(T).Name} ({instance.GetType().Name})");
        if (_services.ContainsKey(typeof(T)))
        {
            throw new InvalidOperationException($"A service of type  {typeof(T).Name} has already been added.");
        }
        _services.Add(typeof(T), new(instance));
        return this;
    }

    public IAppBuilder AddService<TInterface, TConcrete>(TConcrete instance) where TConcrete : class, TInterface, IService
    {
        Logger.Trace<AppBuilder>($"Add Service {typeof(TConcrete).Name} : {typeof(TInterface).Name} ({instance.GetType().Name})");
        if (_services.ContainsKey(typeof(TInterface)))
        {
            throw new InvalidOperationException($"A service of interface {typeof(TInterface).Name} has already been added.");
        }

        if (_services.ContainsKey(typeof(TConcrete)))
        {
            throw new InvalidOperationException($"A service of type {typeof(TConcrete).Name} has already been added.");
        }
        _services.Add(typeof(TConcrete), new(instance));
        _services.Add(typeof(TInterface), new(instance));
        return this;
    }
    public IAppBuilder AddModule<T>() where T : IModule
    {
        var module = Module.CreateFromType<T>();
        Logger.Trace<AppBuilder>($"Add module {module.Name}");
        if (_modules.Any(m => m.Type == module.Type))
        {
            throw new InvalidOperationException($"A module of type {module.Type.AssemblyQualifiedName} has already been added.");
        }

        var result = module.Build(this, appConfig);
        if (!result)
        {
            throw new InvalidOperationException($"Failed to build module. Name = {module.Name}");
        }
        _modules.Add(module);
        return this;
    }

    public IAppBuilder AddConfig<T>(T config) where T : IConfiguration
    {
        if (_configurations.Any(c => c.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"A configuration of type {typeof(T).Name} has already been added.");
        }
        _configurations.Add(ConfigurationDescriptor.Create(config));
        return this;
    }

    public IAppBuilder AddPersistedConfig<T>(T config) where T : IConfiguration, IPersistable<T>
    {
        if (_configurations.Any(c => c.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"A configuration of type {typeof(T).Name} has already been added.");
        }
        _configurations.Add(ConfigurationDescriptor.CreatePersisted(config));
        return this;
    }

    public void BuildAndRun()
    {
        var services = _services.ToFrozenDictionary();
        var configurations = _configurations.ToImmutableArray();
        var modules = _modules.ToImmutableArray();

        if (_runner == null)
        {
            throw new InvalidOperationException("No runner has been set.");
        }

        try
        {
            new TitanApp(services, modules, configurations, _runner)
                .Run();
        }
        catch (Exception e)
        {
            Logger.Error<AppBuilder>($"An unahandled exception was thrown from the App. Type = {e.GetType().Name}. Message = {e.Message}");
            Logger.Error<AppBuilder>(e.StackTrace ?? "[Stacktrace Missing]");
        }
    }

    public T GetService<T>() where T : class, IService
        => _services[typeof(T)].As<T>();

    public IAppBuilder UseRunner<T>() where T : IRunner
    {
        _runner = T.Create();
        return this;
    }
}
