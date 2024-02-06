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

    private IMemoryManager? _memoryManager;
    public bool Init(IMemoryManager memoryManager, ImmutableArray<UnmanagedResourceDescriptor> descriptors)
    {
        if (descriptors.Length == 0)
        {
            Logger.Warning<UnmanagedResourceRegistry>("No unmanaged resources have been registered.");
            return true;
        }

        var size = (uint)descriptors.Sum(static d => d.Size);
        var alignedSize = (uint)descriptors.Sum(static d => d.AlignedSize);

        Logger.Trace<UnmanagedResourceRegistry>($"A total of {descriptors.Length} unmanaged resources. Size = {size} bytes. Total Size (Aligned) = {alignedSize} bytes.");
        if (!memoryManager.TryAllocBuffer(out _resources, alignedSize))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate memory. Size = {alignedSize} bytes");
            return false;
        }

        var offsetLength = descriptors.Length + 1;
        if (!memoryManager.TryAllocArray(out _offsets, (uint)offsetLength))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate offsets array. Count = {offsetLength} Size = {sizeof(uint) * offsetLength}");
            return false;
        }

        uint offset = 0;
        foreach (var descriptor in descriptors)
        {
            _offsets[descriptor.Id] = offset;
            offset += descriptor.AlignedSize;
        }

        _memoryManager = memoryManager;

        return true;
    }

    public ref T GetResource<T>() where T : unmanaged, IResource
        => ref *GetResourcePointer<T>();

    public T* GetResourcePointer<T>() where T : unmanaged, IResource
    {
        Debug.Assert(T.Id < _offsets.Length, $"The index of the type is out of bounds. Might have forgot to register the type. Type = {typeof(T).Name}");
        var offset = _offsets[T.Id];
        Debug.Assert(offset + sizeof(T) < _resources.Size, "The offset for the type exceeds the resources. Why?");
        var ptr = _resources.AsPointer() + offset;
        return (T*)ptr;
    }

    public void Shutdown()
    {
        _memoryManager?.FreeArray(ref _offsets);
        _memoryManager?.FreeBuffer(ref _resources);
        _memoryManager = null;
    }
}
