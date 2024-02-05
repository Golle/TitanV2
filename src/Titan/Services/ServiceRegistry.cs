using System.Collections.Immutable;
using System.Diagnostics;
using Titan.Core;

namespace Titan.Services;

internal class ServiceRegistry : IManagedServices
{
    private readonly Dictionary<Type, ServiceDescriptor> _services;

    public ServiceRegistry(ImmutableArray<ServiceDescriptor> descriptors)
    {
        _services = new(descriptors.Length + 2)
        {
            //NOTE(Jens): Add self to the dictionary
            { typeof(IManagedServices), new ServiceDescriptor(this, typeof(IManagedServices)) },
            { typeof(ServiceRegistry), new ServiceDescriptor(this, typeof(ServiceRegistry)) }
        };
        foreach (var descriptor in descriptors)
        {
            _services.Add(descriptor.Type, descriptor);
        }
    }

    public T GetService<T>() where T : class, IService
    {
        Debug.Assert(_services.ContainsKey(typeof(T)));
        return _services[typeof(T)].As<T>();
    }

    public ManagedResource<T> GetHandle<T>() where T : class, IService
    {
        Debug.Assert(_services.ContainsKey(typeof(T)));
        return _services[typeof(T)].AsHandle<T>();
    }
}
