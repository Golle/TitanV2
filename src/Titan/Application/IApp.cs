using System.Collections.Immutable;
using Titan.Application.Configurations;
using Titan.Application.Services;

namespace Titan.Application;

public interface IApp
{
    T GetService<T>() where T : class, IService;
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;
    void UpdateConfig<T>(T config) where T : IConfiguration;

    internal ImmutableArray<ConfigurationDescriptor> GetConfigurations();
}
