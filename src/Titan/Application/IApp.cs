namespace Titan.Application;

public interface IApp
{
    T GetService<T>() where T : IService;
}