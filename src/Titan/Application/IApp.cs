namespace Titan.Application;

public interface IApp
{
    T GetService<T>() where T : IService;
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;
}
