using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Core;
using Titan.Core.Strings;
using Titan.Resources;
using Titan.Services;

namespace Titan.Systems;


[AttributeUsage(AttributeTargets.Method)]
public sealed class SystemAttribute(SystemStage Stage = SystemStage.Update) : Attribute;

public enum SystemStage
{
    First,
    PreUpdate,
    Update,
    PostUpdate,
    Last
}

public unsafe ref struct SystemInitializer(IUnmanagedResources unmanagedResources, IManagedServices managedServices, Span<uint> mutable, Span<uint> readOnly)
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
    public ManagedResource<T> GetService<T>() where T : class, IService
    {
        //NOTE(Jens): Managed services are not tracked.
        return managedServices.GetHandle<T>();
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SystemDescriptor
{
    public StringRef Name;
    public delegate*<SystemInitializer, void> Init;
    public delegate*<void> Execute;
}

public interface ISystem
{
    static abstract int GetSystems(Span<SystemDescriptor> descriptors);
}
