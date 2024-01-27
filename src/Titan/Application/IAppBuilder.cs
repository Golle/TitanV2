namespace Titan.Application;

public interface IAppBuilder
{
    IAppBuilder AddService<T>(T instance) where T : class, IService;
    IAppBuilder AddService<TInterface, TConcrete>(TConcrete instance) where TConcrete : class, TInterface, IService;
    IAppBuilder AddModule<T>() where T : IModule;
    IAppBuilder AddConfig<T>(T config) where T : IConfiguration;
    void BuildAndRun();

    /// <summary>
    /// Internal method for retrieving other services, this should only be used by internal system that is initialized during build
    /// </summary>
    /// <returns>The service</returns>
    internal T GetService<T>() where T : IService;
}