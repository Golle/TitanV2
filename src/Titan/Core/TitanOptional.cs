namespace Titan.Core;

public readonly unsafe struct TitanOptional<T>(T* ptr) where T : unmanaged
{
    public ref T AsRef() => ref *ptr;
    public T* AsPtr() => ptr;
    public bool HasValue => ptr != null;

    public static implicit operator TitanOptional<T>(T* ptr) => new(ptr);
    public static implicit operator T*(TitanOptional<T> optional) => optional.AsPtr();
}
