using Titan.Resources;

namespace Titan.Core;

public readonly unsafe struct UnmanagedResource<T>(T* resource) where T : unmanaged, IResource
{
    public ref readonly T AsReadOnlyRef => ref AsRef;
    public ref T AsRef => ref *resource;
    public T* AsPointer => resource;

    public static implicit operator T*(in UnmanagedResource<T> resource) => resource.AsPointer;
}
