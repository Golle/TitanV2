using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Core;

[DebuggerDisplay("Handle: {Value}")]
[StructLayout(LayoutKind.Sequential, Pack = sizeof(int))]
public readonly struct Handle<T>(int value) where T : unmanaged
{
    public readonly int Value = value;
    public static readonly Handle<T> Invalid = default;

    public bool Equals(in Handle<T> other)
        => Value == other.Value;

    public override bool Equals(object? obj)
        => throw new InvalidOperationException($"Don't use the Equals with boxing when comparing the {nameof(Handle<T>)}");

    public override int GetHashCode()
        => Value;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in Handle<T> lh, in Handle<T> rh) => lh.Value == rh.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in Handle<T> lh, in Handle<T> rh) => lh.Value != rh.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator uint(in Handle<T> handle) => (uint)handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(in Handle<T> handle) => handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nuint(in Handle<T> handle) => (nuint)handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(in Handle<T> handle) => handle.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle<T>(in uint handle) => new((int)handle);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle<T>(in int handle) => new(handle);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle<T>(in nuint handle) => new((int)handle);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle<T>(in nint handle) => new((int)handle);

#if DEBUG
    public override string ToString()
        => Value.ToString();
#endif
}

public readonly struct Handle(nuint value)
{
    public readonly nuint Value = value;
    public bool IsValid => Value != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nuint(in Handle handle) => handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(in Handle handle) => (nint)handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle(nuint handle) => new(handle);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Handle(nint handle) => new((nuint)handle);
}
