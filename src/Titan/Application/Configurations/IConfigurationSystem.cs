using System.Text.Json.Serialization.Metadata;
using Titan.Application.Services;

namespace Titan.Application.Configurations;

internal record struct ConfigurationDescriptor(IConfiguration Config, string? Filename, JsonTypeInfo? TypeInfo)
{
    public static ConfigurationDescriptor Create<T>(T config) where T : IConfiguration
        => new(config, null, null);

    public static ConfigurationDescriptor CreatePersisted<T>(T config) where T : IConfiguration, IPersistable<T>
        => new(config, T.Filename, T.TypeInfo);
}

public interface IConfigurationSystem : IService
{
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;

    void UpdateConfig<T>(T config) where T : IConfiguration;
}
