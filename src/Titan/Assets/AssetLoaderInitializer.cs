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
    private readonly ReadOnlySpan<AssetRegistry> _registries;
    public IMemoryManager MemoryManager => _serviceRegistry.GetService<IMemoryManager>();
    public IConfigurationManager ConfigurationManager => _serviceRegistry.GetService<IConfigurationManager>();

    internal AssetLoaderInitializer(UnmanagedResourceRegistry unmanagedResource, ServiceRegistry serviceRegistry, ReadOnlySpan<AssetRegistry> registries)
    {
        _unmanagedResource = unmanagedResource;
        _serviceRegistry = serviceRegistry;
        _registries = registries;
    }

    public UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource
        => _unmanagedResource.GetResourceHandle<T>();

    public unsafe T* GetResourcePointer<T>() where T : unmanaged, IResource
        => _unmanagedResource.GetResourcePointer<T>();

    public ManagedResource<T> GetServiceHandle<T>() where T : class, IService
        => _serviceRegistry.GetHandle<T>();
    public T GetService<T>() where T : class, IService
        => _serviceRegistry.GetService<T>();

    public uint GetAssetCountByType(AssetType type)
    {
        var count = 0u;
        foreach (ref readonly var registry in _registries)
        {
            foreach (ref readonly var descriptor in registry.GetAssetDescriptors())
            {
                if (descriptor.Type == type)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
