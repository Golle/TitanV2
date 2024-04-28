using System.Diagnostics;
using Titan.Assets;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Threading;
using Titan.Events;
using Titan.IO.FileSystem;
using Titan.Resources;
using Titan.Services;
using Titan.Systems;

namespace Titan.Application;

internal sealed class TitanApp : IApp, IRunnable
{
    private readonly ServiceRegistry _registry;
    public TitanApp(
        ServiceRegistry registry,
        AppConfig config,
        IReadOnlyList<UnmanagedResourceDescriptor> resources,
        IReadOnlyList<ConfigurationDescriptor> configurations,
        IReadOnlyList<SystemDescriptor> systems,
        IReadOnlyList<AssetRegistryDescriptor> assetRegistries,
        IReadOnlyList<AssetLoaderDescriptor> assetLoaders)
    {
        using var _ = new MeasureTime<TitanApp>("Titan App base system Init completed in {0} ms");
        _registry = registry;
        var memoryManager = registry.GetService<IMemoryManager>();
        var fileSystem = registry.GetService<IFileSystem>();

        var unmanagedResourceRegistry = registry.GetService<UnmanagedResourceRegistry>();
        var configurationManager = registry.GetService<ConfigurationManager>();
        var eventSystem = registry.GetService<EventSystem>();
        var assetsManager = registry.GetService<AssetsManager>();

        // Set up all unmanaged resources that have been registered.
        if (!unmanagedResourceRegistry.Init(memoryManager, resources))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(UnmanagedResourceRegistry)}.");
            throw new InvalidOperationException($"{nameof(UnmanagedResourceRegistry)} failed.");
        }

        // Init the configurations
        if (!configurationManager.Init(fileSystem, configurations))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(ConfigurationManager)}.");
            throw new InvalidOperationException($"{nameof(ConfigurationManager)} failed.");
        }

        if (!eventSystem.Init(memoryManager, config.EventConfig, unmanagedResourceRegistry.GetResourceHandle<EventState>()))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(EventSystem)}.");
            throw new InvalidOperationException($"{nameof(EventSystem)} failed.");
        }

        ref var scheduler = ref unmanagedResourceRegistry.GetResource<SystemsScheduler>();
        if (!scheduler.Init(memoryManager, eventSystem, systems, unmanagedResourceRegistry, registry))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(SystemsScheduler)}.");
            throw new InvalidOperationException($"{nameof(SystemsScheduler)} failed.");
        }

        if (!assetsManager.Init(assetRegistries, unmanagedResourceRegistry.GetResourceHandle<AssetsContext>(), assetLoaders, memoryManager))
        {
            Logger.Error<AppBuilder>($"Failed to init the {nameof(AssetsManager)}.");
            throw new InvalidOperationException($"{nameof(AssetsManager)} failed.");
        }
    }

    public T GetService<T>() where T : class, IService
        => _registry.GetService<T>();

    public ManagedResource<T> GetServiceHandle<T>() where T : class, IService
        => _registry.GetHandle<T>();

    public unsafe UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource =>
        new(_registry.GetService<UnmanagedResourceRegistry>()
            .GetResourcePointer<T>());

    public T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>
        => GetService<ConfigurationManager>().GetConfigOrDefault<T>();

    public void UpdateConfig<T>(T config) where T : IConfiguration =>
        GetService<ConfigurationManager>().UpdateConfig(config);

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
        ref readonly var lifetime = ref GetResourceHandle<ApplicationLifetime>().AsReadOnlyRef;

        var jobSystem = GetService<IJobSystem>();
        ref var scheduler = ref GetResourceHandle<SystemsScheduler>().AsRef;

        Startup(ref scheduler, jobSystem);
        Init(ref scheduler, jobSystem);

        var frameCount = 0;
        var timer = Stopwatch.StartNew();
        Logger.Trace<TitanApp>("Starting main game loop");
        while (lifetime.Active)
        {
            scheduler.UpdateSystems(jobSystem);

            frameCount++;
            if (timer.Elapsed.TotalSeconds > 1f)
            {
                var fps = frameCount / timer.Elapsed.TotalSeconds;
                //Logger.Info<TitanApp>($"FPS: {fps}");
                frameCount = 0;
                timer.Restart();
            }
        }

        Shutdown(ref scheduler, jobSystem);
        EndOfLife(ref scheduler, jobSystem);

        Cleanup();

    }

    private void EndOfLife(ref SystemsScheduler scheduler, IJobSystem jobSystem)
    {
        using (new MeasureTime<TitanApp>("EndOfLife completed in {0} ms"))
        {
            Logger.Trace<TitanApp>("End of life");
            scheduler.EndOfLifeSystems(jobSystem);
        }
    }

    private void Startup(ref SystemsScheduler scheduler, IJobSystem jobSystem)
    {
        using (new MeasureTime<TitanApp>("Startup completed in {0} ms."))
        {
            Logger.Trace<TitanApp>("Startup");
            scheduler.StartupSystems(jobSystem);
        }
    }

    private static void Init(ref SystemsScheduler scheduler, IJobSystem jobSystem)
    {
        using (new MeasureTime<TitanApp>("Init completed in {0} ms."))
        {
            Logger.Trace<TitanApp>("PreInit");
            scheduler.PreInitSystems(jobSystem);
            Logger.Trace<TitanApp>("Init");
            scheduler.InitSystems(jobSystem);
        }

        using (new MeasureTime<TitanApp>("GC Collect completed in {0} ms."))
        {
            GC.Collect();
        }
    }

    private void Shutdown(ref SystemsScheduler scheduler, IJobSystem jobSystem)
    {
        Logger.Trace<TitanApp>("Shutdown");
        scheduler.ShutdownSystems(jobSystem);
        Logger.Trace<TitanApp>("PostShutdown");
        scheduler.PostShutdownSystems(jobSystem);
    }

    private void Cleanup()
    {
        _registry
            .GetService<AssetsManager>()
            .Shutdown();

        var memoryManager = _registry.GetService<IMemoryManager>();
        GetResourceHandle<SystemsScheduler>().AsRef.Shutdown(memoryManager);
        _registry.GetService<EventSystem>().Shutdown();
        _registry.GetService<ConfigurationManager>().Shutdown();
        _registry.GetService<UnmanagedResourceRegistry>().Shutdown();
        _registry.GetService<IFileSystem>().Shutdown();
        _registry.GetService<JobSystem>().Shutdown();
    }
}

