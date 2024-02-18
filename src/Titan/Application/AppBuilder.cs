using Titan.Configurations;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Events;
using Titan.IO.FileSystem;
using Titan.Resources;
using Titan.Runners;
using Titan.Services;
using Titan.Systems;

namespace Titan.Application;


internal class AppBuilder(AppConfig appConfig) : IAppBuilder
{
    //NOTE(Jens): Dictionaries will be faster, but probably not worth it.
    private readonly HashSet<Type> _modules = new();

    private readonly List<ConfigurationDescriptor> _configurations = new();
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
        //var module = ModuleDescriptor.CreateFromType<T>();
        var type = typeof(T);
        Logger.Trace<AppBuilder>($"Add module {type.Name}");
        if (!_modules.Add(type))
        {
            throw new InvalidOperationException($"A module of type {type.AssemblyQualifiedName} has already been added.");
        }
        var result = T.Build(this, appConfig);
        if (!result)
        {
            throw new InvalidOperationException($"Failed to build module. Name = {type.Name}");
        }
        _modules.Add(type);
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

    public IAppBuilder AddSystems<T>() where T : ISystem
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
        if (_runner == null)
        {
            throw new InvalidOperationException("No runner has been set.");
        }

        var memoryManager = GetService<IMemoryManager>();
        var fileSystem = GetService<IFileSystem>();
        var jobSystem = GetService<IJobSystem>();

        var scheduler = GetService<SystemsScheduler>();
        var unmanagedResourceRegistry = GetService<UnmanagedResourceRegistry>();
        var configurationManager = GetService<ConfigurationManager>();
        var eventSystem = GetService<EventSystem>();

        // Set up all unmanaged resources that have been registered.
        if (!unmanagedResourceRegistry.Init(memoryManager, _unmanagedResources))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(UnmanagedResourceRegistry)}.");
            throw new InvalidOperationException($"{nameof(UnmanagedResourceRegistry)} failed.");
        }

        // Init the configurations
        if (!configurationManager.Init(fileSystem, _configurations))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(ConfigurationManager)}.");
            throw new InvalidOperationException($"{nameof(ConfigurationManager)} failed.");
        }

        if (!eventSystem.Init(memoryManager, appConfig.EventConfig, unmanagedResourceRegistry.GetResourceHandle<EventState>()))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(EventSystem)}.");
            throw new InvalidOperationException($"{nameof(EventSystem)} failed.");
        }

        var serviceRegistry = new ServiceRegistry(_services);

        if (!scheduler.Init(memoryManager, jobSystem, eventSystem, _systems, unmanagedResourceRegistry, serviceRegistry))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(SystemsScheduler)}.");
            throw new InvalidOperationException($"{nameof(EventSystem)} failed.");
        }
        
        return new TitanApp(serviceRegistry, _runner);
    }

    public T GetService<T>() where T : class, IService
        => _services.First(s => s.Type == typeof(T)).As<T>();

    public IAppBuilder UseRunner<T>() where T : IRunner
    {
        _runner = T.Create();
        return this;
    }
}
