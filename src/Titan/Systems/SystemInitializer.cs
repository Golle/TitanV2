using System.Diagnostics;
using System.Runtime.CompilerServices;
using Titan.Core;
using Titan.Events;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;

public unsafe ref struct SystemInitializer(IUnmanagedResources unmanagedResources, IManagedServices managedServices, IEventSystem eventSystem, Span<uint> mutable, Span<uint> readOnly)
{
    private readonly Span<uint> _mutable = mutable;
    private readonly Span<uint> _readOnly = readOnly;
    internal int MutableCount { get; private set; }
    internal int ReadOnlyCount { get; private set; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetMutableResource<T>() where T : unmanaged, IResource
    {
        Debug.Assert(MutableCount < _mutable.Length);
        _mutable[MutableCount++] = T.Id;
        return unmanagedResources.GetResourcePointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* GetReadOnlyResource<T>() where T : unmanaged, IResource
    {
        Debug.Assert(ReadOnlyCount < _readOnly.Length);
        _readOnly[ReadOnlyCount++] = T.Id;
        return unmanagedResources.GetResourcePointer<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EventReader<T> CreateEventReader<T>() where T : unmanaged, IEvent
        => eventSystem.CreateReader<T>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EventWriter CreateEventWriter()
        => eventSystem.CreateWriter();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ManagedResource<T> GetService<T>() where T : class, IService
        => managedServices.GetHandle<T>();
}
