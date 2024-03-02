using System.Diagnostics;
using Titan.Core;

namespace Titan.Services;

internal sealed class ServiceRegistry : IService
{
    private readonly Dictionary<Type, ServiceDescriptor> _services;

    public ServiceRegistry(IReadOnlyList<ServiceDescriptor> descriptors)
    {
        _services = new(descriptors.Count + 1)
        {
            //NOTE(Jens): Add self to the dictionary
            { typeof(ServiceRegistry), new ServiceDescriptor(this, typeof(ServiceRegistry)) }
        };
        foreach (var descriptor in descriptors)
        {
            _services.Add(descriptor.Type, descriptor);
        }
    }

    public T GetService<T>() where T : class, IService
    {
        Debug.Assert(_services.ContainsKey(typeof(T)), $"The type {typeof(T)} has not been registered.");
        return _services[typeof(T)].As<T>();
    }

    public ManagedResource<T> GetHandle<T>() where T : class, IService
    {
        Debug.Assert(_services.ContainsKey(typeof(T)), $"The type {typeof(T)} has not been registered.");
        return _services[typeof(T)].AsHandle<T>();
    }
}
