using System.Collections.Immutable;
using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Resources;

public unsafe interface IUnmanagedResources : IService
{
    ref T GetResource<T>() where T : unmanaged, IResource;
    T* GetResourcePointer<T>() where T : unmanaged, IResource;
}

internal sealed unsafe class UnmanagedResourceRegistry : IUnmanagedResources
{
    private TitanBuffer _resources;
    private TitanArray<uint> _offsets;

    private IMemorySystem? _memorySystem;
    public bool Init(IMemorySystem memorySystem, ImmutableArray<UnmanagedResourceDescriptor> descriptors)
    {
        if (descriptors.Length == 0)
        {
            Logger.Warning<UnmanagedResourceRegistry>("No unmanaged resources have been registered.");
            return true;
        }

        var size = (uint)descriptors.Sum(static d => d.Size);
        var alignedSize = (uint)descriptors.Sum(static d => d.AlignedSize);

        Logger.Trace<UnmanagedResourceRegistry>($"A total of {descriptors.Length} unmanaged resources. Size = {size} bytes. Total Size (Aligned) = {alignedSize} bytes.");
        if (!memorySystem.TryAllocBuffer(out _resources, alignedSize))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate memory. Size = {alignedSize} bytes");
            return false;
        }

        if (!memorySystem.TryAllocArray(out _offsets, (uint)descriptors.Length))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate offsets array. Count = {descriptors.Length} Size = {sizeof(uint) * descriptors.Length}");
            return false;
        }

        _memorySystem = memorySystem;

        return true;
    }

    public ref T GetResource<T>() where T : unmanaged, IResource
        => ref *GetResourcePointer<T>();

    public T* GetResourcePointer<T>() where T : unmanaged, IResource
    {
        Debug.Assert(_offsets.Length < T.Id);
        var offset = _offsets[T.Id];
        Debug.Assert(offset + sizeof(T) < _resources.Size);
        var ptr = _resources.AsPointer() + offset;
        return (T*)ptr;
    }

    public void Shutdown()
    {
        _memorySystem?.FreeArray(ref _offsets);
        _memorySystem?.FreeBuffer(ref _resources);
        _memorySystem = null;
    }
}
