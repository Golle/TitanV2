using System.Diagnostics;
using System.Runtime.InteropServices;
using Titan.Core;

namespace Titan.Services;

internal struct ServiceDescriptor(IService service, Type type) : IDisposable
{
    private GCHandle _handle = GCHandle.Alloc(service);
    public readonly Type Type = type;

    public readonly ManagedResource<T> AsHandle<T>() where T : class, IService
    {
        Debug.Assert(service.GetType().IsAssignableTo(typeof(T)));
        return new(_handle);
    }

    public readonly T As<T>() where T : class, IService
    {
        Debug.Assert(service.GetType().IsAssignableTo(typeof(T)));
        return (T)service;
    }

    public void Dispose()
    {
        if (_handle.IsAllocated)
        {
            _handle.Free();
        }
    }
}
