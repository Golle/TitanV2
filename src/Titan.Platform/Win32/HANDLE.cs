using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Titan.Platform.Win32;

[StructLayout(LayoutKind.Sequential)]

public struct HANDLE
{
    public nuint Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nuint(HANDLE handle) => handle.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator nint(HANDLE handle) => (nint)handle.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HANDLE(nuint handle) => new() { Value = handle };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HANDLE(nint handle) => new() { Value = (nuint)handle };
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsValid() => Value != unchecked((nuint)0xFFFFFFFFFFFFFFFFUL);
}
