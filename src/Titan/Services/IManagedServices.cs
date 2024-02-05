using Titan.Core;

namespace Titan.Services;

public interface IManagedServices : IService
{
    T GetService<T>() where T : class, IService;
    ManagedResource<T> GetHandle<T>() where T : class, IService;
}
