using Titan.Configurations;
using Titan.Core;
using Titan.Core.Memory;
using Titan.Resources;
using Titan.Services;

namespace Titan.Assets;

public readonly ref struct AssetLoaderInitializer
{
    private readonly UnmanagedResourceRegistry _unmanagedResource;
    private readonly ServiceRegistry _serviceRegistry;
    public IMemoryManager MemoryManager => _serviceRegistry.GetService<IMemoryManager>();
    public IConfigurationManager ConfigurationManager => _serviceRegistry.GetService<IConfigurationManager>();

    internal AssetLoaderInitializer(UnmanagedResourceRegistry unmanagedResource, ServiceRegistry serviceRegistry)
    {
        _unmanagedResource = unmanagedResource;
        _serviceRegistry = serviceRegistry;
    }

    public UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource
        => _unmanagedResource.GetResourceHandle<T>();

    public unsafe T* GetResourcePointer<T>() where T : unmanaged, IResource
        => _unmanagedResource.GetResourcePointer<T>();

    public ManagedResource<T> GetServiceHandle<T>() where T : class, IService
        => _serviceRegistry.GetHandle<T>();
    public T GetService<T>() where T : class, IService
        => _serviceRegistry.GetService<T>();

}
