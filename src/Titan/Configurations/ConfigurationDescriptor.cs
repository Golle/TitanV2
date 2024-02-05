using System.Text.Json.Serialization.Metadata;

namespace Titan.Configurations;

internal record struct ConfigurationDescriptor(IConfiguration Config, string? Filename, JsonTypeInfo? TypeInfo)
{
    public static ConfigurationDescriptor Create<T>(T config) where T : IConfiguration
        => new(config, null, null);

    public static ConfigurationDescriptor CreatePersisted<T>(T config) where T : IConfiguration, IPersistable<T>
        => new(config, T.Filename, T.TypeInfo);
}
