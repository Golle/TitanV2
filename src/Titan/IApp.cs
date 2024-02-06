using System.Collections.Immutable;
using Titan.Configurations;
using Titan.Core;
using Titan.Resources;
using Titan.Systems;

namespace Titan;

public interface IRunnable
{
    void Run();
}

public interface IApp
{
    T GetService<T>() where T : class, IService;
    ManagedResource<T> GetServiceHandle<T>() where T : class, IService;
    UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource;
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;
    void UpdateConfig<T>(T config) where T : IConfiguration;


    //NOTE(Jens): I don't like this.
    internal ImmutableArray<ConfigurationDescriptor> GetConfigurations();
    internal ImmutableArray<UnmanagedResourceDescriptor> GetResources();
    internal ImmutableArray<SystemDescriptor> GetSystems();
}
