using Titan.Assets;
using Titan.Resources;
using Titan.Systems;

namespace Titan.Application;

public interface IAppBuilder
{
    IAppBuilder AddService<T>(T instance) where T : class, IService;
    IAppBuilder AddService<TInterface, TConcrete>(TConcrete instance) where TConcrete : class, TInterface, IService where TInterface : IService;
    IAppBuilder AddModule<T>() where T : IModule;
    IAppBuilder AddConfig<T>(T config) where T : IConfiguration;
    IAppBuilder AddPersistedConfig<T>(T config) where T : IConfiguration, IPersistable<T>;
    IAppBuilder AddSystems<T>() where T : ISystem;
    IAppBuilder AddResource<T>() where T : unmanaged, IResource;
    IAppBuilder AddRegistry<T>() where T : unmanaged, IAssetRegistry;
    IAppBuilder AddAssetLoader<T>() where T : unmanaged, IAssetLoader;
    IRunnable Build();

    /// <summary>
    /// Internal method for retrieving other services, this should only be used by internal system that is initialized during build
    /// </summary>
    /// <returns>The service</returns>
    internal T GetService<T>() where T : class, IService;

    /// <summary>
    /// Internal Asset Registry function used for internal engine assets.
    /// </summary>
    internal IAppBuilder AddRegistry<T>(bool engineRegistry) where T : unmanaged, IAssetRegistry;
}
