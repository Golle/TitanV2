using System.Collections.Immutable;
using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Resources;
using Titan.Runners;
using Titan.Services;
using Titan.Systems;

namespace Titan.Application;

internal class AppBuilder(AppConfig appConfig) : IAppBuilder
{
    //NOTE(Jens): Dictionaries will be faster, but probably not worth it.
    private readonly List<ConfigurationDescriptor> _configurations = new();
    private readonly List<ModuleDescriptor> _modules = new();
    private readonly List<UnmanagedResourceDescriptor> _unmanagedResources = new();
    private readonly List<ServiceDescriptor> _services = new();
    private readonly List<SystemDescriptor> _systems = new();
    private IRunner? _runner;

    public IAppBuilder AddService<T>(T instance) where T : class, IService
    {
        Logger.Trace<AppBuilder>($"Add Service {typeof(T).Name} ({instance.GetType().Name})");
        if (_services.Any(s => s.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"A service of type  {typeof(T).Name} has already been added.");
        }
        _services.Add(new(instance, typeof(T)));
        return this;
    }

    public IAppBuilder AddService<TInterface, TConcrete>(TConcrete instance) where TConcrete : class, TInterface, IService where TInterface : IService
    {
        Logger.Trace<AppBuilder>($"Add Service {typeof(TConcrete).Name} : {typeof(TInterface).Name} ({instance.GetType().Name})");
        if (_services.Any(s => s.GetType() == typeof(TInterface)))
        {
            throw new InvalidOperationException($"A service of interface {typeof(TInterface).Name} has already been added.");
        }

        if (_services.Any(s => s.GetType() == typeof(TConcrete)))
        {
            throw new InvalidOperationException($"A service of type {typeof(TConcrete).Name} has already been added.");
        }

        _services.Add(new(instance, typeof(TConcrete)));
        _services.Add(new(instance, typeof(TInterface)));
        return this;
    }
    public IAppBuilder AddModule<T>() where T : IModule
    {
        var module = ModuleDescriptor.CreateFromType<T>();
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
        if (_configurations.Any(static c => c.Config.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"A configuration of type {typeof(T).Name} has already been added.");
        }
        _configurations.Add(ConfigurationDescriptor.Create(config));
        return this;
    }

    public IAppBuilder AddPersistedConfig<T>(T config) where T : IConfiguration, IPersistable<T>
    {
        if (_configurations.Any(static c => c.Config.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"A configuration of type {typeof(T).Name} has already been added.");
        }
        _configurations.Add(ConfigurationDescriptor.CreatePersisted(config));
        return this;
    }

    public IAppBuilder AddSystems<T>() where T : unmanaged, ISystem
    {
        Span<SystemDescriptor> systems = stackalloc SystemDescriptor[10];
        var count = T.GetSystems(systems);
        _systems.AddRange(systems[..count]);
        return this;
    }

    public IAppBuilder AddResource<T>() where T : unmanaged, IResource
    {
        if (_unmanagedResources.Any(r => r.Id == T.Id))
        {
            throw new InvalidOperationException($"A resource of type {typeof(T).Name} (Id = {T.Id}) has already been added.");
        }
        var descriptor = UnmanagedResourceDescriptor.Create<T>();
        _unmanagedResources.Add(descriptor);
        return this;
    }

    public IRunnable Build()
    {
        var services = new ServiceRegistry(_services.ToImmutableArray());
        var configurations = _configurations.ToImmutableArray();
        var modules = _modules.ToImmutableArray();
        var unmanagedResources = _unmanagedResources.ToImmutableArray();
        var systems = _systems.ToImmutableArray();

        if (_runner == null)
        {
            throw new InvalidOperationException("No runner has been set.");
        }

        return new TitanApp(services, modules, configurations, unmanagedResources, systems, _runner);
    }

    public T GetService<T>() where T : class, IService
        => _services.First(s => s.Type == typeof(T)).As<T>();

    public IAppBuilder UseRunner<T>() where T : IRunner
    {
        _runner = T.Create();
        return this;
    }
}
