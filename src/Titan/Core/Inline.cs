using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core;

#pragma warning disable CS0169 
[InlineArray(Length)]
public struct Inline2<T> where T : unmanaged
{
    private const int Length = 2;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}


[InlineArray(Length)]
public struct Inline3<T> where T : unmanaged
{
    private const int Length = 3;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}


[InlineArray(Length)]
public struct Inline4<T> where T : unmanaged
{
    private const int Length = 4;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}


[InlineArray(Length)]
public struct Inline8<T> where T : unmanaged
{
    private const int Length = 8;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}

[InlineArray(Length)]
public struct Inline10<T> where T : unmanaged
{
    private const int Length = 10;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}

[InlineArray(Length)]
public struct Inline16<T> where T : unmanaged
{
    private const int Length = 16;
    private T _ref;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref _ref, Length);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe T* AsPointer() => (T*)Unsafe.AsPointer(ref this);
    public unsafe ref T this[uint index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref *(AsPointer() + index);
    }
}
