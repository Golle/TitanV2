using System.Runtime.CompilerServices;

namespace Titan.Platform.Win32;

/// <summary>
/// Helper struct to be able to store pointers in arrays, because it's not possible in C#..
/// </summary>
/// <typeparam name="T">The pointer type</typeparam>
/// <param name="ptr">The pointer value</param>
[SkipLocalsInit]
public readonly unsafe struct Ptr<T>(T* ptr) where T : unmanaged
{
    private readonly T* _value = ptr;
    public bool IsNull => _value == null;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T* Get() => _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(in Ptr<T> ptr) => ptr._value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Ptr<T>(T* ptr) => new(ptr);
}
