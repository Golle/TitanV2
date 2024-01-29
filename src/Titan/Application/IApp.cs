namespace Titan.Application;

public interface IApp
{
    T GetService<T>() where T : IService;
    T GetConfigOrDefaulte<T>() where T : IConfiguration, IDefault<T>;
}
