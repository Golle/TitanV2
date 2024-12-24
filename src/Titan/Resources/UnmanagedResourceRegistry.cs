using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;

namespace Titan.Resources;

internal sealed unsafe class UnmanagedResourceRegistry : IService
{
    private TitanBuffer _resources;
    private TitanArray<uint> _offsets;

    private IMemoryManager? _memoryManager;
    public uint HighestId { get; private set; }
    public bool Init(IMemoryManager memoryManager, IReadOnlyList<UnmanagedResourceDescriptor> descriptors)
    {
        var length = descriptors.Count;
        if (length == 0)
        {
            Logger.Warning<UnmanagedResourceRegistry>("No unmanaged resources have been registered.");
            return true;
        }

        var size = (uint)descriptors.Sum(static d => d.Size);
        var alignedSize = (uint)descriptors.Sum(static d => d.AlignedSize);

        Logger.Trace<UnmanagedResourceRegistry>($"A total of {length} unmanaged resources. Size = {size} bytes. Total Size (Aligned) = {alignedSize} bytes.");
        if (!memoryManager.TryAllocBuffer(out _resources, alignedSize))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate memory. Size = {alignedSize} bytes");
            return false;
        }

        var offsetLength = length + 1;
        if (!memoryManager.TryAllocArray(out _offsets, (uint)offsetLength))
        {
            Logger.Error<UnmanagedResourceRegistry>($"Failed to allocate offsets array. Count = {offsetLength} Size = {sizeof(uint) * offsetLength}");
            return false;
        }

        uint offset = 0;
        foreach (var descriptor in descriptors)
        {
            var id = descriptor.Id;
            _offsets[id] = offset;
            offset += descriptor.AlignedSize;

            HighestId = Math.Max(id, HighestId);
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
        Debug.Assert(offset + sizeof(T) <= _resources.Size, "The offset for the type exceeds the resources. Why?");
        var ptr = _resources.AsPointer() + offset;
        return (T*)ptr;
    }

    /// <summary>
    /// Check if the resource ID is within the range of the registered resources.
    /// <remarks>Might be inaccurate</remarks>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns>True if ID is within the range</returns>
    public bool HasResource<T>() where T : unmanaged, IResource 
        => T.Id < _offsets.Length;

    public UnmanagedResource<T> GetResourceHandle<T>() where T : unmanaged, IResource
        => new(GetResourcePointer<T>());

    public void Shutdown()
    {
        _memoryManager?.FreeArray(ref _offsets);
        _memoryManager?.FreeBuffer(ref _resources);
        _memoryManager = null;
    }
}
