using System.Diagnostics;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Threading;
using Titan.Resources;
using Titan.Services;
using Titan.Systems;

namespace Titan.Application;

internal sealed class TitanApp(ServiceRegistry serviceRegistry) : IApp, IRunnable
{
    public T GetService<T>() where T : class, IService
        => serviceRegistry.GetService<T>();

    public ManagedResource<T> GetServiceHandle<T>() where T : class, IService
        => serviceRegistry.GetHandle<T>();

    public unsafe UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource =>
        new(serviceRegistry.GetService<UnmanagedResourceRegistry>()
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
        var jobSystem = GetService<IJobSystem>();
        var scheduler = GetService<SystemsScheduler>();
        ref var executionTree = ref scheduler._executionTree;

        Logger.Trace<TitanApp>("PreInit");
        executionTree.PreInit(jobSystem);
        Logger.Trace<TitanApp>("Init");
        executionTree.Init(jobSystem);

        Logger.Trace<TitanApp>("Init complete. Doing a GC Collect.");
        var gcTimer = Stopwatch.StartNew();
        GC.Collect();
        gcTimer.Stop();
        Logger.Trace<TitanApp>($"GC Collect completed in {gcTimer.Elapsed.TotalMilliseconds} ms");

        int i = 1000;
        Logger.Trace<TitanApp>("Starting main game loop");
        while (true) //TODO(Jens): Add something that can exit this
        {
            //Thread.Sleep(100);
            executionTree.Update(jobSystem);
        }

        Logger.Trace<TitanApp>("Shutdown");
        executionTree.Shutdown(jobSystem);
        Logger.Trace<TitanApp>("PostShutdown");
        executionTree.PostShutdown(jobSystem);
    }
}

