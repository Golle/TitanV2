using System.Collections.Immutable;
using Titan.Configurations;

namespace Titan.Application;

public interface IApp
{
    T GetService<T>() where T : IService;
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;
    void UpdateConfig<T>(T config) where T : IConfiguration;

    internal ImmutableArray<ConfigurationDescriptor> GetConfigurations();
}
