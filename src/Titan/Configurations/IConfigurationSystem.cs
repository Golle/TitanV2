namespace Titan.Configurations;

public interface IConfigurationSystem : IService
{
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;

    void UpdateConfig<T>(T config) where T : IConfiguration;
}
